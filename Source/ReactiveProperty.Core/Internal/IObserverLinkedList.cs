namespace Reactive.Bindings.Internal
{
    internal interface IObserverLinkedList<T>
    {
        void UnsubscribeNode(ObserverNode<T> node);
    }
}
