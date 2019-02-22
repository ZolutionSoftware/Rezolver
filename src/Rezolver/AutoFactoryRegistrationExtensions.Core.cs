using Rezolver.Targets;
using System;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Extensions for registering auto factories
    /// </summary>
    public static partial class AutoFactoryRegistrationExtensions
    {
        /// <summary>
        /// Registers an <see cref="AutoFactoryTarget"/> for the given delegate type <typeparamref name="TDelegate"/>, enabling 
        /// the container to automatically build and inject a delegate which uses the container to produce a result.
        /// 
        /// This method and its overload can be used for any delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate to be injected. Must have a non-void return type
        /// and all its parameters must be of distinct types.</typeparam>
        /// <param name="targets">Required. The target container in which the registration is to be made.</param>
        public static void RegisterAutoFactory<TDelegate>(this IRootTargetContainer targets)
            where TDelegate : Delegate
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(typeof(TDelegate));

            if (!IsValidReturnType(returnType))
                throw new ArgumentException($"Type argument {typeof(TDelegate)} for {nameof(TDelegate)} is invalid - must be a delegate with a non-void return type.", nameof(TDelegate));

            if (!AreValidParameterTypes(parameterTypes))
                throw new ArgumentException($"The parameter types for {typeof(TDelegate)} are invalid - they must all be unique", nameof(TDelegate));

            RegisterAutoFactoryInternal(targets, typeof(TDelegate), returnType, parameterTypes);
        }

        /// <summary>
        /// Registers an <see cref="AutoFactoryTarget"/> for the given <paramref name="delegateType"/>, enabling 
        /// the container to automatically build and inject a delegate which uses the container to produce a result.
        /// 
        /// This method and its overload can be used for any delegate type.
        /// </summary>
        /// <param name="targets">Required. The target container in which the registration is to be made.</param>
        /// <param name="delegateType">Required. Must have a non-void return type and all its parameters must be of distinct types.</param>
        public static void RegisterAutoFactory(this IRootTargetContainer targets, Type delegateType)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            // assume the type is a valid delegate type
            var (returnType, parameterTypes) = TypeHelpers.DecomposeDelegateType(delegateType);

            if (!IsValidReturnType(returnType))
                throw new ArgumentException($"{delegateType} is invalid - must be a delegate with a non-void return value", nameof(delegateType));

            if (!AreValidParameterTypes(parameterTypes))
                throw new ArgumentException($"The parameter types for {delegateType} are invalid - they must all be unique, non ref/out types", nameof(delegateType));

            RegisterAutoFactoryInternal(targets, delegateType, returnType, parameterTypes);
        }

        private static bool IsValidReturnType(Type returnType)
        {
            return returnType != typeof(void);
        }

        private static bool AreValidParameterTypes(Type[] parameterTypes)
        {
            if (parameterTypes.Length == 0)
                return true;

            if (parameterTypes.Distinct().Count() != parameterTypes.Length)
                return false;

            if (parameterTypes.Any(pt => pt.IsByRef))
                return false;

            return true;
        }

        private static void RegisterAutoFactoryInternal(IRootTargetContainer targets, Type delegateType, Type returnType, Type[] parameterTypes)
        {
            // create an unbound AutoFactoryTarget and register it.
            // this will also trigger a projection to be registered from IEnumerable<ReturnType> to IEnumerable<DelegateType>
            // since the newly created & registered target is unbound.
            targets.Register(new AutoFactoryTarget(delegateType, returnType, parameterTypes));
        }
    }
}
