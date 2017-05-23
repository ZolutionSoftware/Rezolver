using System;

namespace Rezolver.Options
{
    internal class OptionContainer<TOption> : IDirectTarget, ITarget
    {
        public TOption Option { get; }

        bool ITarget.UseFallback => false;

        Type ITarget.DeclaredType => typeof(OptionContainer<TOption>);

        ScopeBehaviour ITarget.ScopeBehaviour => ScopeBehaviour.None;

        ScopePreference ITarget.ScopePreference => ScopePreference.Current;

        public OptionContainer(TOption option)
        {
            Option = option;
        }

        object IDirectTarget.GetValue() => this;

        bool ITarget.SupportsType(Type type)
        {
            // yes - this is a bit weird.  It's because of the way that service-specific options
            // are registered.
            return (typeof(OptionContainer<TOption>) == type
                || (TypeHelpers.IsGenericType(type)
                    && typeof(OptionContainer<,>).Equals(type.GetGenericTypeDefinition())
                    && typeof(TOption) == TypeHelpers.GetGenericArguments(type)[1]));
        }
    }
}
