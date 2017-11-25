// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
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
        internal class BindableCollectionType
        {
            public Type Type { get; }
            public MethodInfo AddMethod { get; }
            public Type ElementType { get; }

            public BindableCollectionType(Type type, MethodInfo addMethod, Type elementType)
            {
                Type = type;
                AddMethod = addMethod;
                ElementType = elementType;
            }
        }

        private static readonly ConcurrentDictionary<Type, BindableCollectionType> _collectionTypeCache 
            = new ConcurrentDictionary<Type, BindableCollectionType>();

        internal static IEnumerable<Type> GetAllBases(this Type type)
        {
            var baseType = TypeHelpers.BaseType(type);
            while (baseType != null)
            {
                yield return baseType;
                baseType = TypeHelpers.BaseType(baseType);
            }
        }

        private static Type MakeVectorType(Type elementType, int ignored)
        {
            return TypeHelpers.MakeArrayType(elementType);
        }

        private static Type MakeArrayType(Type elementType, int rank)
        {
            return TypeHelpers.MakeArrayType(elementType, rank);
        }

        /// <summary>
        /// Returns a non-null result if the <paramref name="type"/> supports IEnumerable&lt;T&gt;
        /// and has a public Add method which returns void and which takes a single parameter of
        /// the type equal to the IEnumerable element type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsBindableCollectionType(this Type type)
        {
            return GetBindableCollectionTypeInfo(type) != null;
        }

        internal static BindableCollectionType GetBindableCollectionTypeInfo(this Type type)
        {
            return _collectionTypeCache.GetOrAdd(type, t => {
                Type enumElemType;
                MethodInfo addMethod;
                var allAddMethods = t.GetRuntimeMethods()
                    .Where(mi => mi.IsPublic && mi.Name == "Add" && mi.ReturnType == typeof(void))
                    .Select(mi => new { Method = mi, Parameters = mi.GetParameters() });

                foreach (var i in TypeHelpers.GetInterfaces(t)
                    .Where(iface => TypeHelpers.IsGenericType(iface) && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    enumElemType = TypeHelpers.GetGenericArguments(i)[0];
                    addMethod = allAddMethods.Where(
                        m => (m.Parameters?.Length ?? 0) == 1 && m.Parameters[0].ParameterType == enumElemType)
                        .Select(m => m.Method)
                        .SingleOrDefault();

                    if (addMethod != null)
                        return new BindableCollectionType(t, addMethod, enumElemType);
                }
                return null;
            });
        }

        /// <summary>
        /// Note - no verification.  if the type is not an array type, then bad
        /// things happen.  This method automatically handles a vector/multi-dim
        /// array type mismatch between vectors (0-based 1 dimensional array) and
        /// rank 1 multidimensional arrays as described here in this SO: 
        /// https://stackoverflow.com/q/45693868 
        /// (answered by GOAT Skeet of course).
        /// 
        /// Note that the function only returns down to object[]: Array and Object
        /// are excluded.
        /// </summary>
        /// <param name="arrayType"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetBaseArrayTypes(this Type arrayType)
        {
            var elemType = TypeHelpers.GetElementType(arrayType);
            if (elemType == typeof(object)) return Enumerable.Empty<Type>();
            var rank = TypeHelpers.GetArrayRank(arrayType);
            Func<Type, int, Type> typeFac = rank == 1 ? (Func<Type, int, Type>)MakeVectorType : MakeArrayType;
            List<Type> toReturn = new List<Type>();
            while(elemType != typeof(object))
            {
                elemType = TypeHelpers.BaseType(elemType);
                toReturn.Add(typeFac(elemType, rank));
            }
            return toReturn;
        }

        internal static IEnumerable<Type> GetInterfaceArrayTypes(this Type interfaceArrayType)
        {
            var elemType = TypeHelpers.GetElementType(interfaceArrayType);
            var rank = TypeHelpers.GetArrayRank(interfaceArrayType);
            Func<Type, int, Type> typeFac = rank == 1 ? (Func<Type, int, Type>)MakeVectorType : MakeArrayType;
            List<Type> toReturn = new List<Type>();
            foreach(var iFaceType in TypeHelpers.GetInterfaces(elemType))
            {
                toReturn.Add(typeFac(iFaceType, rank));
            }
            return toReturn;
        }

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
#if MAXCOMPAT
			return type.GetTypeInfo().DeclaredFields.Where(f => !f.IsStatic).ToArray();
#else
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
		}

		internal static PropertyInfo[] GetInstanceProperties(this Type type)
		{
#if MAXCOMPAT
			return type.GetTypeInfo().DeclaredProperties.Where(p => 
				(p.GetMethod != null && !p.GetMethod.IsStatic) ||
				(p.SetMethod != null && !p.SetMethod.IsStatic)).ToArray();
#else
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif

		}

		internal static PropertyInfo[] GetStaticProperties(this Type type)
		{
#if MAXCOMPAT
			return type.GetTypeInfo().DeclaredProperties.Where(p => 
				(p.GetMethod != null && p.GetMethod.IsStatic) ||
				(p.SetMethod != null && p.SetMethod.IsStatic)).ToArray();
#else
			return type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#endif
		}

		internal static FieldInfo[] GetStaticFields(this Type type)
		{
#if MAXCOMPAT
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
