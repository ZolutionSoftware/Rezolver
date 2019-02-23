// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver.Options
{
    internal class OptionContainer<TOption> : IDirectTarget, ITarget, IOptionContainer<TOption>
    {
        public Guid Id { get; } = Guid.NewGuid();

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
            // yes - this is a bit weird.  The whole type compatibility thing here is subverted,
            // so we can register an option against a service, taking advantage of the contravariance
            // and other generic functionality of the target containers -
            return typeof(IOptionContainer<TOption>) == type
                || (type.IsGenericType
                    && (typeof(IOptionContainer<,>).Equals(type.GetGenericTypeDefinition())
                        && typeof(TOption) == type.GetGenericArguments()[1])
                    || (typeof(IAnyGenericOptionContainer<>).Equals(type.GetGenericTypeDefinition())
                        && typeof(TOption) == type.GetGenericArguments()[0]));
        }
    }
}
