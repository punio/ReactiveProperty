using System;
using System.ComponentModel;



namespace Reactive.Bindings
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveProperty<T> : IObservable<T>, IDisposable, INotifyPropertyChanged
    {
        T Value { get; set; }
    }
}
