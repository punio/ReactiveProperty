using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Reactive.Bindings
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    [System.Runtime.CompilerServices.TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ReactiveCollectionSync<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable subscription;
        private readonly IScheduler scheduler;

        [NonSerialized]
        private int blockReentrancyCount;
        [NonSerialized]
        private bool skipRaisingEvents;
        [NonSerialized]
        private readonly List<NotifyCollectionChangedEventArgs> collectionChangedArgsList = new List<NotifyCollectionChangedEventArgs>();

        /// <summary>
        /// Operate scheduler is UIDispatcherScheduler.
        /// </summary>
        public ReactiveCollectionSync()
            : this(ReactivePropertyScheduler.Default)
        { }

        /// <summary>
        /// Operate scheduler is argument's scheduler.
        /// </summary>
        public ReactiveCollectionSync(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            subscription = Disposable.Empty;
        }

        /// <summary>
        /// Source sequence as ObservableCollection. Operate scheduler is UIDispatcherScheduler.
        /// </summary>
        public ReactiveCollectionSync(IObservable<T> source)
            : this(source, ReactivePropertyScheduler.Default)
        {
        }

        /// <summary>
        /// Source sequence as ObservableCollection. Operate scheduler is argument's scheduler.
        /// </summary>
        public ReactiveCollectionSync(IObservable<T> source, IScheduler scheduler)
        {
            this.scheduler = scheduler;
            subscription = source.ObserveOn(scheduler).Subscribe(Add);
        }

        public void Dispose() => subscription.Dispose();


        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            scheduler.Schedule(() => PropertyChanged?.Invoke(this, e));
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;



        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            lock (collectionChangedArgsList) collectionChangedArgsList.Add(e);

            scheduler.Schedule(() =>
            {
                var handler = CollectionChanged;
                blockReentrancyCount++;
                try
                {
                    lock (collectionChangedArgsList)
                    {
                        foreach (var args in collectionChangedArgsList)
                        {
                            handler?.Invoke(this, args);
                        }
                        collectionChangedArgsList.Clear();
                    }
                }
                finally
                {
                    blockReentrancyCount--;
                }
            });
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, index));
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList oldItems, IList newItems, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems, index));
        }
        private void OnCollectionReset() => OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Count property
        /// </summary>
        private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Indexer property
        /// </summary>
        private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (blockReentrancyCount > 0)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if (CollectionChanged?.GetInvocationList().Length > 1)
                    throw new InvalidOperationException("Cannot change ObservableCollection during a CollectionChanged event.");
            }
        }


        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T removedItem = this[oldIndex];

            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, removedItem);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            CheckReentrancy();
            base.ClearItems();
            OnCountPropertyChanged();
            OnIndexerPropertyChanged();
            OnCollectionReset();
        }


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            CheckReentrancy();
            T removedItem = this[index];

            base.RemoveItem(index);

            if (!skipRaisingEvents)
            {
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
            }
        }


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);

            if (!skipRaisingEvents)
            {
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
        }


        public void AddRange(params T[] items)
        {
            skipRaisingEvents = true;
            var index = Count;
            foreach (var item in items) { Add(item); }
            skipRaisingEvents = false;

            if (items.Length > 0)
            {
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Add, items, index);
            }
        }

        public void AddRange(IEnumerable<T> items) => AddRange(items.ToArray());


        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            CheckReentrancy();
            T originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }

}
