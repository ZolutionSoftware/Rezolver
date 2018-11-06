using Rezolver.Targets;
using System;

namespace Rezolver
{
    public static partial class AutoFactoryRegistrationExtensions
    {
        public static void RegisterAutoFactory<TDelegate>(this IRootTargetContainer targets)
            where TDelegate : Delegate
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            // TODO: validate the delegate type's return value.
        }

        private static void RegisterAutoFactory(IRootTargetContainer targets, Type delegateType)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            // assume the type is a valid delegate type
            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(delegateType);

            if (returnType == null || returnType == typeof(void))
                throw new ArgumentException("Delegate type must have a non-void return value", nameof(delegateType));

            EnableAutoFactoryInternal(targets, delegateType, returnType, parameterTypes);
        }

        private static void EnableAutoFactoryInternal(IRootTargetContainer targets, Type delegateType, Type returnType, Type[] parameterTypes)
        {
            targets.AddKnownType(delegateType);
            // we need this to support auto-producing of enumerables of factories
            targets.RegisterProjection(returnType, delegateType, (root, source) => new AutoFactoryTarget(source, delegateType, returnType, parameterTypes));
        }
    }
}
