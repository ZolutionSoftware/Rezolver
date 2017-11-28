using Rezolver;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
    internal static class RzMethodBaseExtensions
    {

        internal static MethodInfo ToGenericTypeDefMethod(this MethodInfo method)
        {
            if (TypeHelpers.IsGenericType(method.DeclaringType))
            {
#if MAXCOMPAT
                // THIS IS THE ONLY WAY I CAN FIND TO DO THIS WHEN MethodHandle IS NOT AVAILABLE
                var position = Array.IndexOf(
                    TypeHelpers.GetAllMethods(method.DeclaringType),
                    method);
                if (position >= 0)
                {
                    return TypeHelpers.GetAllMethods(method.DeclaringType.GetGenericTypeDefinition())[position];
                }
#else
                // BETTER WAY:
                return (MethodInfo)MethodBase.GetMethodFromHandle(method.MethodHandle,
                    method.DeclaringType.GetGenericTypeDefinition().TypeHandle);
#endif
            }
            return null;
        }

        internal static ConstructorInfo ToGenericTypeDefCtor(this ConstructorInfo ctor)
        {
            if (TypeHelpers.IsGenericType(ctor.DeclaringType))
            {
#if MAXCOMPAT
                // THIS IS THE ONLY WAY I CAN FIND TO DO THIS WHEN MethodHandle IS NOT AVAILABLE
                // See https://stackoverflow.com/questions/47445250/get-generic-constructor-from-closed-version-net-standard-1-1
                var position = Array.IndexOf(
                    TypeHelpers.GetAllConstructors(ctor.DeclaringType),
                    ctor);
                if (position >= 0)
                {
                    return TypeHelpers.GetAllConstructors(ctor.DeclaringType.GetGenericTypeDefinition())[position];
                }
#else
                // BETTER WAY:
                return (ConstructorInfo)MethodBase.GetMethodFromHandle(ctor.MethodHandle,
                    ctor.DeclaringType.GetGenericTypeDefinition().TypeHandle);
#endif
            }

            return null;
        }


        internal static MethodInfo ToGenericTypeMethod(this MethodInfo method, Type targetGenericType)
        {
            if (TypeHelpers.IsGenericType(targetGenericType) && targetGenericType.GetGenericTypeDefinition() == method.DeclaringType)
            {
#if MAXCOMPAT
                // THIS IS THE ONLY WAY I CAN FIND TO DO THIS WHEN MethodHandle IS NOT AVAILABLE
                var position = Array.IndexOf(
                    TypeHelpers.GetAllMethods(method.DeclaringType),
                    method);
                if (position >= 0)
                {
                    return TypeHelpers.GetAllMethods(targetGenericType)[position];
                }
#else
                // BETTER WAY:
                return (MethodInfo)MethodBase.GetMethodFromHandle(method.MethodHandle,
                    targetGenericType.TypeHandle);
#endif
            }
            return null;
        }

        internal static ConstructorInfo ToGenericTypeCtor(this ConstructorInfo ctor, Type targetGenericType)
        {
            if (TypeHelpers.IsGenericType(targetGenericType) && targetGenericType.GetGenericTypeDefinition() == ctor.DeclaringType)
            {
#if MAXCOMPAT
                // THIS IS THE ONLY WAY I CAN FIND TO DO THIS WHEN MethodHandle IS NOT AVAILABLE
                var position = Array.IndexOf(
                    TypeHelpers.GetAllConstructors(ctor.DeclaringType),
                    ctor);
                if (position >= 0)
                {
                    return TypeHelpers.GetAllConstructors(targetGenericType)[position];
                }
#else
                // BETTER WAY:
                return (ConstructorInfo)MethodBase.GetMethodFromHandle(ctor.MethodHandle,
                    targetGenericType.TypeHandle);
#endif
            }
            return null;
        }
    }
}
