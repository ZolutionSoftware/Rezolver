// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

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
        internal static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(Type type, bool inherit=false)
            where TAttribute : Attribute
        {
#if DOTNET
            return type.GetTypeInfo().GetCustomAttributes<TAttribute>(inherit);
#else
            return type.GetCustomAttributes<TAttribute>(inherit);
#endif
        } 

        internal static bool IsArray(Type type)
        {
#if DOTNET
            return type.GetTypeInfo().IsArray;
#else
            return type.IsArray;
#endif
        }

        internal static int GetArrayRank(Type type)
        {
#if DOTNET
            return type.GetTypeInfo().GetArrayRank();
#else
            return type.GetArrayRank();
#endif
        }

        /// <summary>
        /// Note - will use non-rank overload of underlying MakeArrayType if rank is null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        internal static Type MakeArrayType(Type type, int? rank = null)
        {
#if DOTNET
            return rank != null ? type.GetTypeInfo().MakeArrayType(rank.Value) : type.GetTypeInfo().MakeArrayType();
#else
            
            return rank != null ? type.MakeArrayType(rank.Value) : type.MakeArrayType();
#endif
        }

        internal static Type GetElementType(Type type)
        {
#if DOTNET
            return type.GetTypeInfo().GetElementType();
#else
            return type.GetElementType();
#endif
        }

        internal static bool IsSubclassOf(Type type, Type superClass)
        {
#if DOTNET
            return type.GetTypeInfo().IsSubclassOf(superClass);
#else
            return type.IsSubclassOf(superClass);
#endif
        }

        internal static bool IsPublic(Type type)
		{
#if DOTNET
			return type.GetTypeInfo().IsPublic;
#else
			return type.IsPublic;
#endif
		}
		internal static bool ContainsGenericParameters(Type type)
		{
#if DOTNET
			return type.GetTypeInfo().ContainsGenericParameters;
#else
			return type.ContainsGenericParameters;
#endif
		}

		internal static bool IsGenericType(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsGenericType;
#else
			return type.IsGenericType;
#endif
		}

		internal static bool IsGenericTypeDefinition(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
			return type.IsGenericTypeDefinition;
#endif
		}

        internal static GenericParameterAttributes GetGenericParameterAttributes(Type type)
        {
#if DOTNET
            return type.GetTypeInfo().GenericParameterAttributes;
#else
			return type.GenericParameterAttributes;
#endif
        }
        
        internal static bool IsValueType(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsValueType;
#else
			return type.IsValueType;
#endif
		}

		internal static Type BaseType(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}

        internal static bool AreCompatible(Type from, Type to)
		{
			from.MustNotBeNull("from");
			to.MustNotBeNull("to");
			//this is checking whether it's possible to do a runtime cast between the
			//two types.  Now, this is more than just reference casting - as the runtime
			//will support 'int? a = null' for example, or 'int? a = 1' for example.

			if (IsAssignableFrom(to, from))
				return true;

            return @from.IsNullableType(out Type nulledType) && IsAssignableFrom(to, nulledType);
        }

		internal static bool IsInterface(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsInterface;
#else
			return type.IsInterface;
#endif
		}

		internal static IEnumerable<Type> GetInterfaces(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().ImplementedInterfaces;
#else
			return type.GetInterfaces();
#endif
		}

		internal static Type[] GetGenericArguments(Type type)
		{
#if DOTNET
			//the new TypeInfo system doesn't return parameters and arguments for Generic Types and their Definitions via the same
			//property as before.  So we need to know whether it's a generic type or generic type definition in order to get the
			//correct list.
			return type.IsConstructedGenericType ? type.GetTypeInfo().GenericTypeArguments : type.GetTypeInfo().GenericTypeParameters;
#else
			return type.GetGenericArguments();
#endif
		}

		internal static bool IsAbstract(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsAbstract;
#else
			return type.IsAbstract;
#endif
		}

		internal static bool IsClass(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().IsClass;
#else
			return type.IsClass;
#endif
		}

		internal static bool IsAssignableFrom(Type to, Type from)
		{
#if DOTNET
            return to.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
#else
			return to.IsAssignableFrom(from);
#endif
		}

		internal static Assembly GetAssembly(Type type)
		{
#if DOTNET
            return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}

		/// <summary>
		/// Gets a public constructor whose parameter types match those provided.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		internal static ConstructorInfo GetConstructor(Type type, Type[] types)
		{
#if DOTNET
			return GetConstructors(type).Where(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types)).SingleOrDefault();
#else
			return type.GetConstructor(types);
#endif
		}

		/// <summary>
		/// Gets all public constructors declared on the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static ConstructorInfo[] GetConstructors(Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic).Cast<ConstructorInfo>().ToArray();
#else
			return type.GetConstructors();
#endif
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
#if DOTNET
			//can't use GetDeclaredMethod because it does public and non-public methods, instance and static.
			try
			{
				return type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic && !m.IsStatic && m.Name == methodName).SingleOrDefault();
			}
			catch(InvalidOperationException ioex)
			{
				throw new InvalidOperationException($"More than one method on the type {type} found with the name {methodName}", ioex);
			}
#else
			return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
#endif
		}
	}
}
