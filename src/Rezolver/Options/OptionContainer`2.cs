using System;

namespace Rezolver.Options
{
    /// <summary>
    /// Note that this type is used in a strange way - the Options extension 'FetchDirect' with this 
    /// type, but treat the result as its base because for instances where TService is an open
    /// generic, it's not possible to construct an instance of the generic type.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TOption"></typeparam>
    internal class OptionContainer<TService, TOption> : OptionContainer<TOption>
    {
        public OptionContainer(TOption option) : base(option) { }
    }
}
