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

		internal static bool CanBeNull(Type type)
		{
			return !IsValueType(type) || IsNullableType(type);
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

			Type nulledType = null;
			return IsNullableType(@from, out nulledType) && IsAssignableFrom(to, nulledType);
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
            return type.GetTypeInfo().GenericTypeArguments;
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
			return GetConstructors(type).Where(c => c.GetParameters().SequenceEqual(types));
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
			return type.GetTypeInfo().DeclaredMethods.Where(c => c.IsConstructor && c.IsPublic).Cast<ConstructorInfo>().ToArray();
#else
			return type.GetConstructors();
#endif
		}

		/// <summary>
		/// Gets all public instance fields declared on the type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static FieldInfo[] GetPublicFields(Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredFields.Where(f => !f.IsStatic && f.IsPublic).ToArray();
#else
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
#endif
		}

		/// <summary>
		/// Gets all publicly readable and/or writable instance properties 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static PropertyInfo[] GetPublicProperties(Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredProperties.Where(p => 
				(p.GetMethod != null && p.GetMethod.IsPublic && !p.GetMethod.IsStatic) ||
				(p.SetMethod != null && p.SetMethod.IsPublic && !p.SetMethod.IsStatic)).ToArray();
#else
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif
		}

		internal static bool IsNullableType(Type type)
		{
			return IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		internal static bool IsNullableType(Type type, out Type nulledType)
		{
			nulledType = null;

			if (!IsGenericType(type))
				return false;
			var genType = type.GetGenericTypeDefinition();
			if (genType != typeof(Nullable<>))
				return false;

			nulledType = GetGenericArguments(type)[0];
			return true;
		}

		internal static bool IsEnumerableType(Type type, out Type elementType)
		{
			elementType = null;
			if (!IsGenericType(type))
				return false;
			var genDef = type.GetGenericTypeDefinition();
			if (genDef != typeof(IEnumerable<>))
				return false;

			elementType = GetGenericArguments(type)[0];
			return true;
		}

		internal static IEnumerable<Type> GetAllBases(Type t)
		{
			t.MustNotBeNull("t");
			var baseType = BaseType(t);
			while (baseType != null)
			{
				yield return baseType;
				baseType = BaseType(baseType);
			}
		}
	}
}
