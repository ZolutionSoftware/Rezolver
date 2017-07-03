// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for System.Type
	/// 
	/// Note - these are specifically new functions added to Type by Rezolver,
	/// not 'ghost' extensions for the type members that are missing under certain
	/// compilation profiles.
	/// 
	/// They are in the TypeHelpers static class.
	/// </summary>
	internal static class TypeExtensions
	{
        internal static bool IsContravariantTypeParameter(this Type type)
        {
            return type.IsGenericParameter &&
                (TypeHelpers.GetGenericParameterAttributes(type) & GenericParameterAttributes.Contravariant)
                == GenericParameterAttributes.Contravariant;
        }

        internal static bool IsCovariantTypeParameter(this Type type)
        {
            return type.IsGenericParameter &&
                (TypeHelpers.GetGenericParameterAttributes(type) & GenericParameterAttributes.Covariant)
                == GenericParameterAttributes.Covariant;
        }

        internal static bool IsVariantTypeParameter(this Type type)
        {
            switch (TypeHelpers.GetGenericParameterAttributes(type)
                       & GenericParameterAttributes.VarianceMask)
            {
                case GenericParameterAttributes.Contravariant:
                case GenericParameterAttributes.Covariant:
                    return true;
                default:
                    return false;
            }
        }

        internal static string CSharpLikeTypeName(this Type type)
        {
            StringBuilder sb = new StringBuilder();
            CSharpLikeTypeName(type, sb);
            return sb.ToString();
        }

        internal static void CSharpLikeTypeName(this Type t, StringBuilder sb)
        {
            if (sb == null) sb = new StringBuilder();
            sb.Append(t.Name);
            if (TypeHelpers.IsGenericType(t))
            {
                sb.Append("<");
                bool moreThanOne = false;
                foreach (var tP in TypeHelpers.GetGenericArguments(t))
                {
                    if (moreThanOne)
                        sb.Append(", ");

                    CSharpLikeTypeName(tP, sb);

                    moreThanOne = true;
                }
                sb.Append(">");
            }
        }

        internal static bool CanBeNull(this Type type)
		{
			return !TypeHelpers.IsValueType(type) || IsNullableType(type);
		}


		internal static bool IsNullableType(this Type type)
		{
			return TypeHelpers.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		internal static bool IsNullableType(this Type type, out Type nulledType)
		{
			nulledType = null;

			if (!TypeHelpers.IsGenericType(type))
				return false;
			var genType = type.GetGenericTypeDefinition();
			if (genType != typeof(Nullable<>))
				return false;

			nulledType = TypeHelpers.GetGenericArguments(type)[0];
			return true;
		}

		/// <summary>
		/// Gets all public instance fields declared on the type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static FieldInfo[] GetInstanceFields(this Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredFields.Where(f => !f.IsStatic).ToArray();
#else
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
		}

		internal static PropertyInfo[] GetInstanceProperties(this Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredProperties.Where(p => 
				(p.GetMethod != null && !p.GetMethod.IsStatic) ||
				(p.SetMethod != null && !p.SetMethod.IsStatic)).ToArray();
#else
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif

		}

		internal static PropertyInfo[] GetStaticProperties(this Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredProperties.Where(p => 
				(p.GetMethod != null && p.GetMethod.IsStatic) ||
				(p.SetMethod != null && p.SetMethod.IsStatic)).ToArray();
#else
			return type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#endif
		}

		internal static FieldInfo[] GetStaticFields(this Type type)
		{
#if DOTNET
			return type.GetTypeInfo().DeclaredFields.Where(f => f.IsStatic).ToArray();
#else
			return type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#endif
		}

		internal static bool IsEnumerableType(this Type type, out Type elementType)
		{
			elementType = null;
			if (!TypeHelpers.IsGenericType(type))
				return false;
			var genDef = type.GetGenericTypeDefinition();
			if (genDef != typeof(IEnumerable<>))
				return false;

			elementType = TypeHelpers.GetGenericArguments(type)[0];
			return true;
		}
	}


}
