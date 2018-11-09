using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
    /// <summary>
    /// A target that builds a delegate (potentially parameterised) by wrapping another target, typically
    /// resolved from the container.
    /// 
    /// When the delegate is executed, the delegate will execute the same code as would've been executed
    /// had the dependency from the inner target been directly injected.  This allows applications which use factory delegates 
    /// to build components to use Rezolver containers as the mechanism for doing this directly instead of having
    /// to resort to mechanisms such as <see cref="IServiceProvider"/> etc.
    /// </summary>
    public class AutoFactoryTarget : TargetBase, INotifyRegistrationTarget
    {
        /// <summary>
        /// The delegate type that will be built by this target.
        /// </summary>
        public override Type DeclaredType => DelegateType;

        /// <summary>
        /// If the factory is already bound to another target (e.g. <see cref="ConstructorTarget"/>), then this
        /// will be it.  If not, then a compiler should use the <see cref="Bind"/>
        /// </summary>
        public ITarget BoundTarget { get; }
        public Type DelegateType { get; }
        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateType"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="boundTarget">Optional - the target whose result will be produced by the factory, if known.</param>
        public AutoFactoryTarget(Type delegateType, Type returnType, Type[] parameterTypes, ITarget boundTarget = null)
            : base(boundTarget?.Id ?? Guid.NewGuid()) // note here - passing the ID in from the inner target to preserve the order.
        {
            DelegateType = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ParameterTypes = parameterTypes ?? TypeHelpers.EmptyTypes;
            if (ParameterTypes.Distinct().Count() != ParameterTypes.Length)
                throw new ArgumentException($"Invalid auto factory delegate type: {delegateType} - all parameter types must be unique", nameof(parameterTypes));

            BoundTarget = boundTarget;
        }

        /// <summary>
        /// Returns the <see cref="ITarget"/> that should be compiled as the body of the delegate.  If <see cref="BoundTarget"/>
        /// is not null and its <see cref="ITarget.UseFallback"/> is false, then no binding is performed.
        /// 
        /// Otherwise, a suitable target is sought from the <paramref name="compileContext"/>.  If not found, then
        /// a <see cref="ResolvedTarget"/> is emitted with a fallback set to the <see cref="BoundTarget"/> if non-null,
        /// meaning that the delegate will actually perform a late-bound resolve operation on the container on which it's
        /// called.
        /// </summary>
        /// <param name="compileContext">The current compilation context, this is used to resolve the target to be bound.</param>
        /// <returns>The target to be compiled as the body of the factory.</returns>
        public ITarget Bind(ICompileContext compileContext)
        {
            if (BoundTarget != null && !BoundTarget.UseFallback)
                return BoundTarget;

            return compileContext.Fetch(ReturnType) ?? new ResolvedTarget(ReturnType, BoundTarget);
        }

        // when registering an unbound target, we also automatically register
        // a projection for the delegate type which is fed by an enumerable of the return
        // type of the delegate.  So IEnumerable<Func<Foo>> <== IEnumerable<Foo>
        void INotifyRegistrationTarget.OnRegistration(IRootTargetContainer root, Type registeredType)
        {
            if (this.BoundTarget == null) {
                root.RegisterProjection(ReturnType, registeredType, (root2, source) => new AutoFactoryTarget(registeredType, ReturnType, ParameterTypes, source));
            }
        }
    }
}
