using Rezolver.Compilation;
using System;
using System.Linq;

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
    /// <remarks>This is used to implement the [Autofactories functionality](/developers/docs/autofactories.html).</remarks>
    public class AutoFactoryTarget : TargetBase, INotifyRegistrationTarget
    {
        private readonly Type _delegateType;
        /// <summary>
        /// The delegate type that will be built by this target - always equal to <see cref="DeclaredType"/>
        /// </summary>
        public override Type DeclaredType => _delegateType;

        /// <summary>
        /// If the target is already bound to another target (e.g. <see cref="ConstructorTarget"/>), then this
        /// will be it.  
        /// 
        /// Note that the compiler should always call the <see cref="Bind(ICompileContext)"/> method
        /// in any case to get the inner target.
        /// </summary>
        public ITarget BoundTarget { get; }

        /// <summary>
        /// The return type of the <see cref="DeclaredType"/>
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// An array of types of the arguments that are accepted by delegates of the type <see cref="DeclaredType"/>,
        /// or, an empty array if the the delegate is nullary.
        /// </summary>
        public Type[] ParameterTypes { get; }

        /// <summary>
        /// Creates a new <see cref="AutoFactoryTarget"/> for the given <paramref name="delegateType"/>, optionally already bound
        /// to the given <paramref name="boundTarget"/>.
        /// </summary>
        /// <param name="delegateType">Required.  The type of delegate that is to be produced by this target.  It MUST have a non-void return type.</param>
        /// <param name="boundTarget">Optional.  The target whose result is to be wrapped by the delegate.</param>
        public AutoFactoryTarget(Type delegateType, ITarget boundTarget = null)
            : base(boundTarget?.Id ?? NextId())
        {
            if (delegateType == null)
                throw new ArgumentNullException(nameof(delegateType));
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("Must be a delegate type with a non-void return type", nameof(delegateType));

            var (returnType, paramTypes) = TypeHelpers.DecomposeDelegateType(delegateType);

            if (returnType == typeof(void))
                throw new ArgumentException("Delegate return type must be non-void", nameof(delegateType));

            if(paramTypes.Distinct().Count() != paramTypes.Length)
                throw new ArgumentException($"Invalid auto factory delegate type: {delegateType} - all parameter types must be unique", nameof(delegateType));

            _delegateType = delegateType;
            ReturnType = returnType;
            ParameterTypes = paramTypes;
            BoundTarget = boundTarget;
        }

        internal AutoFactoryTarget(Type delegateType, Type returnType, Type[] parameterTypes, ITarget boundTarget = null)
            : base(boundTarget?.Id ?? NextId()) // note here - passing the ID in from the inner target to preserve the order.
        {
            _delegateType = delegateType;
            ReturnType = returnType;
            ParameterTypes = parameterTypes ?? Type.EmptyTypes;
            if (ParameterTypes.Distinct().Count() != ParameterTypes.Length)
                throw new ArgumentException($"Invalid auto factory delegate type: {delegateType} - all parameter types must be unique", nameof(parameterTypes));

            BoundTarget = boundTarget;
        }

        /// <summary>
        /// Overrides the base implementation to enhance support for closed generics where
        /// the <see cref="DeclaredType"/> is an open generic.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if this target can build an instance of the <paramref name="type"/>, otherwise <c>false</c></returns>
        public override bool SupportsType(Type type)
        {
            if (base.SupportsType(type))
                return true;

            // if it doesn't match, then we could be dealing with a scenario
            // where the target type is Func<T<Foo>> and where this is bound to
            // Func<T<>> (or something similar).  In this case, we have to check the
            // return

            if (!type.IsGenericType || type.ContainsGenericParameters)
                return false;   //we don't bind to open generics

            if (type.GetGenericTypeDefinition() != typeof(Func<>))
                return false;

            // get the desired return type
            var expectedReturnType = type.GetGenericArguments()[0];

            if (expectedReturnType.IsAssignableFrom(ReturnType))
                return true;

            if (!ReturnType.ContainsGenericParameters)
                return false;

            // if we have an open generic return type, then we're compatible if
            // the desired return type is a generic that's based on that open generic,
            // or another which inherits from it.
            if (!expectedReturnType.IsGenericType)
                return false;
            
            return expectedReturnType.GetGenericTypeDefinition().IsAssignableFrom(ReturnType);
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
                root.RegisterProjection(ReturnType, registeredType, CreateTarget);
            }

            AutoFactoryTarget CreateTarget(IRootTargetContainer root2, ITarget source) => new AutoFactoryTarget(registeredType, ReturnType, ParameterTypes, source);
        }
    }
}
