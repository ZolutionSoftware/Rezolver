// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Targets
{
    /// <summary>
    /// Target used to wrap another when matched against a service type contravariantly or covariantly.
    /// </summary>
    /// <remarks>This target is produced automatically by functionality such as 
    /// <see cref="RootTargetContainerExtensions.FetchAllCompatibleTargets(IRootTargetContainer, Type)"/> for any target
    /// whose registered type is not exactly the same as the type requested.  The target's identity (chiefly determined by 
    /// its <see cref="Id"/>) is always equal to the <see cref="Target"/> that it wraps, thus allowing it to masquerade as
    /// the wrapped target.</remarks>
    public class VariantMatchTarget : ITarget
    {
        /// <summary>
        /// Always returns the <see cref="ITarget.Id"/> of the <see cref="Target"/>, since this target
        /// masquerades as the one that it wraps.
        /// </summary>
        public int Id => Target.Id;

        /// <summary>
        /// Returns the value of the same property returned by the <see cref="Target"/>
        /// </summary>
        public bool UseFallback => Target.UseFallback;

        /// <summary>
        /// Returns the value of the same property returned by the <see cref="Target"/>
        /// </summary>
        public Type DeclaredType => Target.DeclaredType;

        /// <summary>
        /// Returns the value of the same property returned by the <see cref="Target"/>
        /// </summary>
        public ScopeBehaviour ScopeBehaviour => Target.ScopeBehaviour;

        /// <summary>
        /// Returns the value of the same property returned by the <see cref="Target"/>
        /// </summary>
        public ScopePreference ScopePreference => Target.ScopePreference;

        /// <summary>
        /// Type originally requested
        /// </summary>
        public Type RequestedType { get; }

        /// <summary>
        /// Type against which the <see cref="Target"/> was found
        /// </summary>
        public Type RegisteredType { get; }

        /// <summary>
        /// The <see cref="ITarget"/> that's wrapped by this target
        /// </summary>
        public ITarget Target { get; }

        private VariantMatchTarget(ITarget target, Type requestedType,  Type registeredType)
        {
            Target = target;
            RequestedType = requestedType;
            RegisteredType = registeredType;
        }

        /// <summary>
        /// Returns whether the <see cref="Target"/> supports the given <paramref name="type"/>
        /// </summary>
        /// <param name="type">The type to be checked for compatibility.</param>
        /// <returns><c>true</c> if the inner <see cref="Target"/> can build/obtain an instance
        /// of the given <paramref name="type"/>, otherwise <c>false.</c></returns>
        public bool SupportsType(Type type) => Target.SupportsType(type);

        // variant matches shouldn't be nested and the direct targets should never be wrapped.
        internal static bool ShouldWrap(ITarget target) => !(target is IFactoryProvider || target is IInstanceProvider || target is IDirectTarget || target is VariantMatchTarget);

        internal static ITarget Wrap(ITarget target, Type requestedType, Type registeredType) => ShouldWrap(target) ?
            new VariantMatchTarget(target, requestedType, registeredType) : target;
    }
}
