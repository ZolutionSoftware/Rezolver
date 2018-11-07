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

            var (returnType, argTypes) = TypeHelpers.DecomposeDelegateType(type);

            // allow late-bound delegate implementation
            var innerTarget = Root.Fetch(returnType);
            if (innerTarget == null)
                innerTarget = new ResolvedTarget(returnType);

            // create a func target (new type) which wraps the inner target
            return new AutoFactoryTarget(innerTarget, type, returnType, argTypes);
        }
    }
}
