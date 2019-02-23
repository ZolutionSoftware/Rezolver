// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;
using Rezolver;

namespace System.Reflection
{
    internal static class RzMethodBaseExtensions
    {

        internal static MethodInfo ToGenericTypeDefMethod(this MethodInfo method)
        {
            if (TypeHelpers.IsGenericType(method.DeclaringType))
            {

                return (MethodInfo)MethodBase.GetMethodFromHandle(method.MethodHandle,
                    method.DeclaringType.GetGenericTypeDefinition().TypeHandle);
            }

            return null;
        }

        internal static ConstructorInfo ToGenericTypeDefCtor(this ConstructorInfo ctor)
        {
            if (TypeHelpers.IsGenericType(ctor.DeclaringType))
            { 
                return (ConstructorInfo)MethodBase.GetMethodFromHandle(ctor.MethodHandle,
                    ctor.DeclaringType.GetGenericTypeDefinition().TypeHandle);
            }

            return null;
        }

        internal static MethodInfo ToGenericTypeMethod(this MethodInfo method, Type targetGenericType)
        {
            if (TypeHelpers.IsGenericType(targetGenericType) && targetGenericType.GetGenericTypeDefinition() == method.DeclaringType)
            {
                return (MethodInfo)MethodBase.GetMethodFromHandle(method.MethodHandle,
                    targetGenericType.TypeHandle);
            }

            return null;
        }

        internal static ConstructorInfo ToGenericTypeCtor(this ConstructorInfo ctor, Type targetGenericType)
        {
            if (TypeHelpers.IsGenericType(targetGenericType) && targetGenericType.GetGenericTypeDefinition() == ctor.DeclaringType)
            {
                return (ConstructorInfo)MethodBase.GetMethodFromHandle(ctor.MethodHandle,
                    targetGenericType.TypeHandle);
            }

            return null;
        }
    }
}
