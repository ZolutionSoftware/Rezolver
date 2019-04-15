// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Linq;

namespace Rezolver
{
    internal static class TypeHelpers
    {
        private delegate int _DummyDelegate_(int a);
        internal static (Type returnType, Type[] argTypes) DecomposeDelegateType(Type delegateType)
        {
            var method = delegateType.GetPublicInstanceMethod(nameof(_DummyDelegate_.Invoke));

            return (method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }

        /// <summary>
        /// Note - will use non-rank overload of underlying MakeArrayType if rank is null
        /// </summary>
        internal static Type MakeArrayType(Type type, int? rank = null)
        {
            return rank != null ? type.MakeArrayType(rank.Value) : type.MakeArrayType();
        }

        internal static bool AreCompatible(Type from, Type to)
        {
            if (to.IsAssignableFrom(from))
                return true;

            var nulledType = from.GetNullableUnderlyingType();
            return nulledType != null && to.IsAssignableFrom(nulledType);
        }
    }
}
