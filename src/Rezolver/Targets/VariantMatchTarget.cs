using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
    /// <summary>
    /// Target used to wrap another when matched contravariantly or covariantly
    /// </summary>
    public class VariantMatchTarget : ITarget
    {
        public Guid Id => Target.Id;

        public bool UseFallback => Target.UseFallback;

        public Type DeclaredType => Target.DeclaredType;

        public ScopeBehaviour ScopeBehaviour => Target.ScopeBehaviour;

        public ScopePreference ScopePreference => Target.ScopePreference;

        /// <summary>
        /// Type originally requested
        /// </summary>
        public Type RequestedType { get; }
        /// <summary>
        /// Type against which the <see cref="Target"/> was found
        /// </summary>
        public Type RegisteredType { get; }
        public ITarget Target { get; }
        private VariantMatchTarget(ITarget target, Type requestedType,  Type registeredType)
        {
            Target = target;
            RequestedType = requestedType;
            RegisteredType = registeredType;
        }

        public bool SupportsType(Type type) => Target.SupportsType(type);

        // variant matches shouldn't be nested and the direct targets should never be wrapped.
        internal static bool ShouldWrap(ITarget target) => !(target is ICompiledTarget || target is IDirectTarget || target is VariantMatchTarget);

        internal static ITarget Wrap(ITarget target, Type requestedType, Type registeredType) => ShouldWrap(target) ?
            new VariantMatchTarget(target, requestedType, registeredType) : target;
    }
}
