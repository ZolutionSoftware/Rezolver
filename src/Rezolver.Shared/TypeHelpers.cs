// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Concurrent;

namespace Rezolver
{
    /// <summary>
    /// Common pattern now for multi-targeting.  Abstract the Type/TypeInfo class away so we can accommodate
    /// TypeInfo reflection split introduced in 45, which is now the standard in Core CLR, but not supported
    /// across all flavours of PCL libraries.  Unfortunately, this means that reflection using TypeInfo is
    /// significantly slower, unless you cache the TypeInfo reference, because there's an extra method call
    /// to go through before you can start reflecting the type fully.
    /// </summary>
    internal static class TypeHelpers
    {
        public static Type[] EmptyTypes
        {
            get
            {
                return Type.EmptyTypes;
            }
        }
        internal static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(Type type, bool inherit=false)
            where TAttribute : Attribute
        {
            return type.GetCustomAttributes<TAttribute>(inherit);
        }

        internal static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        internal static bool IsDelegateType(Type type) => IsAssignableFrom(typeof(Delegate), type);

        private delegate int _DummyDelegate_(int a);
        internal static (Type returnType, Type[] argTypes) DecomposeDelegateType(Type delegateType)
        {
            var method = GetMethod(delegateType, nameof(_DummyDelegate_.Invoke));

            return (method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }

        internal static int GetArrayRank(Type type)
        {
            return type.GetArrayRank();
        }

        /// <summary>
        /// Note - will use non-rank overload of underlying MakeArrayType if rank is null
        /// </summary>
        internal static Type MakeArrayType(Type type, int? rank = null)
        {
            
            return rank != null ? type.MakeArrayType(rank.Value) : type.MakeArrayType();
        }

        internal static Type GetElementType(Type type)
        {
            return type.GetElementType();
        }

        internal static bool IsSubclassOf(Type type, Type superClass)
        {
            return type.IsSubclassOf(superClass);
        }

        internal static bool IsPublic(Type type)
        {
            return type.IsPublic;
        }

        internal static bool ContainsGenericParameters(Type type)
        {
            return type.ContainsGenericParameters;
        }

        internal static bool IsGenericType(Type type)
        {
            return type.IsGenericType;
        }

        internal static bool IsGenericTypeDefinition(Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        internal static GenericParameterAttributes GetGenericParameterAttributes(Type type)
        {
            return type.GenericParameterAttributes;
        }

        internal static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        internal static Type BaseType(Type type)
        {
            return type.BaseType;
        }

        internal static bool AreCompatible(Type from, Type to)
        {
            from.MustNotBeNull("from");
            to.MustNotBeNull("to");
            // this is checking whether it's possible to do a runtime cast between the
            // two types.  Now, this is more than just reference casting - as the runtime
            // will support 'int? a = null' for example, or 'int? a = 1' for example.

            if (IsAssignableFrom(to, from))
            {
                return true;
            }

            return @from.IsNullableType(out Type nulledType) && IsAssignableFrom(to, nulledType);
        }

        internal static bool IsInterface(Type type)
        {
            return type.IsInterface;
        }

        internal static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetInterfaces();
        }

        internal static Type[] GetGenericArguments(Type type)
        {
            return type.GetGenericArguments();
        }

        internal static bool IsAbstract(Type type)
        {
            return type.IsAbstract;
        }

        internal static bool IsClass(Type type)
        {
            return type.IsClass;
        }

        internal static bool IsAssignableFrom(Type to, Type from)
        {
            return to.IsAssignableFrom(from);
        }

        internal static Assembly GetAssembly(Type type)
        {
            return type.Assembly;
        }

        /// <summary>
        /// Gets a public constructor whose parameter types match those provided.
        /// </summary>
        internal static ConstructorInfo GetConstructor(Type type, Type[] types)
        {
            return type.GetConstructor(types);
        }

        /// <summary>
        /// Gets all public constructors declared on the given type.
        /// </summary>
        internal static ConstructorInfo[] GetConstructors(Type type)
        {
            return type.GetConstructors();
        }

        internal static ConstructorInfo[] GetAllConstructors(Type type)
        {
            return type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets a public instance method whose name matches that passed - regardless
        /// of signature.
        ///
        /// Note - if there is more than one method with this name, then an exception occurs.
        ///
        /// If there is no method with this name, the method returns null.
        /// </summary>
        /// <param name="type">The type whose methods are to be searched.</param>
        /// <param name="methodName">The name of the public instance method that is sought.</param>
        /// <returns></returns>
        internal static MethodInfo GetMethod(Type type, string methodName)
        {
            return GetMethod(type, methodName, true, false);
        }

        internal static MethodInfo GetMethod(Type type, string methodName, bool isPublic, bool isStatic)
        {
            BindingFlags flags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            flags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.DeclaredOnly;
            return type.GetMethod(methodName, flags);
        }

        internal static MethodInfo[] GetAllMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        }
    }
}
