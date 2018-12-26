using System;



namespace Reactive.Bindings
{
    /// <summary>
    /// Mode of ReactiveProperty
    /// </summary>
    [Flags]
    public enum ReactivePropertyMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// If next value is same as current, not set and not notify.
        /// </summary>
        DistinctUntilChanged = 0b0001,

        /// <summary>
        /// Push notify on instance created and subscribed.
        /// </summary>
        RaiseLatestValueOnSubscribe = 0b0010,

        /// <summary>
        /// Ignore initial validation error
        /// </summary>
        IgnoreInitialValidationError = 0b0100,

        /// <summary>
        /// Default mode value. It is same as <see cref="DistinctUntilChanged"/> | <seealso cref="RaiseLatestValueOnSubscribe"/>.
        /// </summary>
        Default = DistinctUntilChanged | RaiseLatestValueOnSubscribe,
    }
}
