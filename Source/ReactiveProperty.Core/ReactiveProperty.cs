using System;
using System.Collections.Generic;
using System.ComponentModel;
using Reactive.Bindings.Internal;



namespace Reactive.Bindings
{
    public class ReactiveProperty<T> : IReactiveProperty<T>, IReadOnlyReactiveProperty<T>, IObserverLinkedList<T>
    {
        public ReactiveProperty(T initialValue = default, ReactivePropertyMode mode = ReactivePropertyMode.Default, IEqualityComparer<T> equalityComparer = default)
        {

        }


        public T Value
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose() => throw new NotImplementedException();

        public IDisposable Subscribe(IObserver<T> observer) => throw new NotImplementedException();

        void IObserverLinkedList<T>.UnsubscribeNode(ObserverNode<T> node) => throw new NotImplementedException();
    }
}
