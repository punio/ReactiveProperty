using System;
using System.Threading;



namespace Reactive.Bindings.Internal
{
    internal sealed class ObserverNode<T> : IObserver<T>, IDisposable
    {
        #region Fields
        private readonly IObserver<T> observer;
        private IObserverLinkedList<T> list;
        #endregion


        #region Properties
        public ObserverNode<T> Previous { get; set; }

        public ObserverNode<T> Next { get; set; }
        #endregion


        #region Constructors
        public ObserverNode(IObserverLinkedList<T> list, IObserver<T> observer)
        {
            this.list = list;
            this.observer = observer;
        }
        #endregion


        #region IObserver<T> implementations
        public void OnNext(T value)
        {
            observer.OnNext(value);
        }

        public void OnError(Exception error)
        {
            observer.OnError(error);
        }

        public void OnCompleted()
        {
            observer.OnCompleted();
        }
        #endregion


        #region IDisposable implementations
        public void Dispose()
        {
            var sourceList = Interlocked.Exchange(ref list, null);
            if (sourceList != null)
            {
                sourceList.UnsubscribeNode(this);
                sourceList = null;
            }
        }
        #endregion
    }
}
