using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Target dictionary container which provides support for automatic production of factory delegates
    /// of any type (so long as they have a return value).
    /// </summary>
    /// <remarks>
    /// This target container is created and registered via the <see cref="Configuration.InjectAutoFactories"/>
    /// </remarks>
    internal class AutoFactoryTargetContainer : TargetDictionaryContainer
    {
        internal AutoFactoryTargetContainer(IRootTargetContainer root)
            : base(root)
        {

        }
        
        private (Type returnType, Type[] argTypes) DecomposeFuncType(Type funcType)
        {
            var method = TypeHelpers.GetMethod(funcType, "Invoke");

            return (method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }

        public override ITarget Fetch(Type type)
        {
            if (!TypeHelpers.IsDelegateType(type))
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

        public override IEnumerable<ITarget> FetchAll(Type type)
        {
            // required to allow interoperability with the FetchAll() functionality; because the targets we return are
            // not in the underlying dictionary, so we have to 
            var baseResult = base.FetchAll(type);
            if (!baseResult.Any())
            {
                // now it's a similar trick to the EnumerableTargetContainer, we have to perform
                // a covariant search for the result type and then project the targets.
                (Type returnType, Type[] argTypes) = DecomposeFuncType(type);

                return Root.FetchAllCompatibleTargetsInternal(returnType)
                    .Select(t => new AutoFactoryTarget(t, type, returnType, argTypes));
            }

            return baseResult;
        }
    }

    internal class AutoFactoriesTargetContainer : ITargetContainer
    {
        public IRootTargetContainer Root { get; }

        public ITargetContainer CombineWith(ITargetContainer existing, Type type)
        {
            throw new NotImplementedException();
        }

        public ITarget Fetch(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITarget> FetchAll(Type type)
        {
            throw new NotImplementedException();
        }

        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotImplementedException();
        }

        public void Register(ITarget target, Type serviceType = null)
        {
            throw new NotImplementedException();
        }

        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotImplementedException();
        }
    }
}
