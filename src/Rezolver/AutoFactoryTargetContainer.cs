using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Target dictionary container which provides support for automatic production of delegates
    /// based on the <see cref="Func{TResult}"/> generic delegate type and its non-unary variants (i.e.
    /// <see cref="Func{T, TResult}"/> and up).
    /// </summary>
    /// <remarks>
    /// The container binds to the open generic delegate type, effectively decorating the container's own
    /// GenericTargetContainer for that type.
    /// </remarks>
    internal class AutoFactoryTargetContainer : TargetDictionaryContainer
    {
        /// <summary>
        /// Will be an open generic delegate type (such as <see cref="Func{TResult}"/>)
        /// </summary>
        public Type GenericFactoryTypeDefinition { get; }


        //private Type[] _objectArgsForFunc;
        internal AutoFactoryTargetContainer(IRootTargetContainer root, Type factoryType)
            : base(root)
        { 
            GenericFactoryTypeDefinition = factoryType;
        }

        internal AutoFactoryTargetContainer(IRootTargetContainer root)
            : base(root)
        {

        }

        //private ITargetContainer EnsureInner()
        //{
        //    // This reuses the same logic that TargetContainer employs via these extension methods
        //    return Inner ??
        //        (Inner = Root.CreateTargetContainerForServiceTypeInternal(GenericFactoryTypeDefinition));
        //}

        //public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        //{
        //    if (existing is AutoFactoryTargetContainer)
        //        return existing;
        //    else if (Inner != null)
        //        throw new NotSupportedException("This target container has already been combined with another");

        //    Inner = existing ?? throw new ArgumentNullException(nameof(existing));
        //    return this;
        //}
        private (Type returnType, Type[] argTypes) DecomposeFuncType(Type funcType)
        {
            var method = TypeHelpers.GetMethod(funcType, "Invoke");

            return (method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
            
            //var typeArgs = TypeHelpers.GetGenericArguments(funcType);

            //return (typeArgs[typeArgs.Length - 1], typeArgs.Take(typeArgs.Length - 1).ToArray());
        }

        public override ITarget Fetch(Type type)
        {
            if (!TypeHelpers.IsAssignableFrom(typeof(Delegate), type))
            {
                throw new ArgumentException($"This target container only supports delegate types - {type} is not a delegate type");
            }

            var result = base.Fetch(type);
            if (result != null && !result.UseFallback)
            {
                return result;
            }

            (Type returnType, Type[] argTypes) = DecomposeFuncType(type);

            // allow late-bound delegate implementation
            var innerTarget = Root.Fetch(returnType);
            if (innerTarget == null)
                innerTarget = new ResolvedTarget(returnType);

            // create a func target (new type) which wraps the inner target
            return new AutoFactoryTarget(innerTarget, type, returnType, argTypes);
        }

        //public IEnumerable<ITarget> FetchAll(Type type)
        //{
        //    // required to allow interoperability with the FetchAll() functionality; because the targets we return are
        //    // not in the underlying dictionary, so we have to 
        //    var baseResult = EnsureInner().FetchAll(type);
        //    if (!baseResult.Any())
        //    {
        //        // now it's a similar trick to the EnumerableTargetContainer, we have to perform
        //        // a covariant search for the result type and then project the targets.
        //        (Type returnType, Type[] argTypes) = DecomposeFuncType(type);

        //        return Root.FetchAllCompatibleTargetsInternal(returnType)
        //            .Select(t => new AutoFactoryTarget(t, type, returnType, argTypes));
        //    }

        //    return baseResult;
        //}

        //public ITargetContainer FetchContainer(Type type)
        //{
        //    return EnsureInner().FetchContainer(type);
        //}

        //public void Register(ITarget target, Type serviceType = null)
        //{
        //    EnsureInner().Register(target, serviceType);
        //}

        //public void RegisterContainer(Type type, ITargetContainer container)
        //{
        //    EnsureInner().RegisterContainer(type, container);
        //}
    }
}
