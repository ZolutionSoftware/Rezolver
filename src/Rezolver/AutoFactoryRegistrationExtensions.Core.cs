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

            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(typeof(TDelegate));

            if (returnType == typeof(void))
                throw new ArgumentException($"Type argument {nameof(TDelegate)} is invalid - must be a delegate with a non-void return type.", nameof(TDelegate));

            RegisterAutoFactoryInternal(targets, typeof(TDelegate), returnType, parameterTypes);
        }

        public static void RegisterAutoFactory(this IRootTargetContainer targets, Type delegateType)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            // assume the type is a valid delegate type
            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(delegateType);

            if (returnType == null || returnType == typeof(void))
                throw new ArgumentException($"{delegateType} is invalid - must be a delegate with a non-void return value", nameof(delegateType));

            RegisterAutoFactoryInternal(targets, delegateType, returnType, parameterTypes);
        }

        private static void RegisterAutoFactoryInternal(IRootTargetContainer targets, Type delegateType, Type returnType, Type[] parameterTypes)
        {
            targets.AddKnownType(delegateType);
            // we need this to support auto-producing of enumerables of factories
            targets.RegisterProjection(returnType, delegateType, (root, source) => new AutoFactoryTarget(source, delegateType, returnType, parameterTypes));
        }
    }
}
