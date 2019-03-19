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
        private static readonly ConcurrentDictionary<Type, BindableCollectionType> CollectionTypeCache
            = new ConcurrentDictionary<Type, BindableCollectionType>();

        internal static IEnumerable<Type> GetAllBases(this Type type)
        {
            if (type.BaseType == null)
                return Enumerable.Empty<Type>();

            List<Type> toReturn = new List<Type>() { type.BaseType };
            type = type.BaseType;
            
            while ((type = type.BaseType) != null)
            {
                toReturn.Add(type);
            }

            return toReturn;
        }

        internal static bool IsBindableCollectionType(this Type type)
        {
            return GetBindableCollectionTypeInfo(type) != null;
        }

        internal static BindableCollectionType GetBindableCollectionTypeInfo(this Type type)
        {
            return CollectionTypeCache.GetOrAdd(type, t =>
            {
                Type enumElemType;
                MethodInfo addMethod;

                var allAddMethods = t.GetAllMethods()
                    .Where(mi => mi.IsPublic && mi.Name == "Add" && mi.ReturnType == typeof(void))
                    .Select(mi => new { Method = mi, Parameters = mi.GetParameters() });

                foreach (var i in t.GetInterfaces()
                    .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    enumElemType = i.GetGenericArguments()[0];
                    addMethod = allAddMethods.Where(
                        m => (m.Parameters?.Length ?? 0) == 1 && m.Parameters[0].ParameterType == enumElemType)
                        .Select(m => m.Method)
                        .SingleOrDefault();

                    if (addMethod != null)
                    {
                        return new BindableCollectionType(t, addMethod, enumElemType);
                    }
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
        /// <param name="arrayType">The type</param>
        /// <returns>An enumerable of types representing the hierarchy of all array types
        /// to which an instance of the arrayType can be assigned.</returns>
        internal static IEnumerable<Type> GetBaseArrayTypes(this Type arrayType)
        {
            var elemType = arrayType.GetElementType();
            if (elemType == typeof(object))
            {
                return Enumerable.Empty<Type>();
            }

            var rank = arrayType.GetArrayRank();
            Func<Type, int, Type> typeFac = rank == 1 ? (Func<Type, int, Type>)MakeVectorType : MakeArrayType;
            List<Type> toReturn = new List<Type>();
            while (elemType != typeof(object))
            {
                elemType = elemType.BaseType;
                toReturn.Add(typeFac(elemType, rank));
            }

            return toReturn;
        }

        internal static IEnumerable<Type> GetInterfaceArrayTypes(this Type interfaceArrayType)
        {
            var elemType = interfaceArrayType.GetElementType();
            var rank = interfaceArrayType.GetArrayRank();
            Func<Type, int, Type> typeFac = rank == 1 ? (Func<Type, int, Type>)MakeVectorType : MakeArrayType;
            List<Type> toReturn = new List<Type>();

            foreach (var iFaceType in elemType.GetInterfaces())
            {
                toReturn.Add(typeFac(iFaceType, rank));
            }

            return toReturn;
        }

        internal static bool IsContravariantTypeParameter(this Type type)
        {
            return type.IsGenericParameter &&
                (type.GenericParameterAttributes & GenericParameterAttributes.Contravariant)
                == GenericParameterAttributes.Contravariant;
        }

        internal static bool IsCovariantTypeParameter(this Type type)
        {
            return type.IsGenericParameter &&
                (type.GenericParameterAttributes & GenericParameterAttributes.Covariant)
                == GenericParameterAttributes.Covariant;
        }

        internal static bool IsVariantTypeParameter(this Type type)
        {
            var masked = type.GenericParameterAttributes
                       & GenericParameterAttributes.VarianceMask;
            return masked == GenericParameterAttributes.Contravariant || masked == GenericParameterAttributes.Covariant;
        }

        internal static string CSharpLikeTypeName(this Type type)
        {
            StringBuilder sb = new StringBuilder();
            CSharpLikeTypeName(type, sb);
            return sb.ToString();
        }

        internal static void CSharpLikeTypeName(this Type t, StringBuilder sb)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }

            sb.Append(t.Name);
            if (t.IsGenericType)
            {
                sb.Append("<");
                bool moreThanOne = false;
                foreach (var tP in t.GetGenericArguments())
                {
                    if (moreThanOne)
                    {
                        sb.Append(", ");
                    }

                    CSharpLikeTypeName(tP, sb);

                    moreThanOne = true;
                }

                sb.Append(">");
            }
        }

        internal static bool CanBeNull(this Type type)
        {
            return !type.IsValueType || IsNullableType(type);
        }

        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static Type GetNullableUnderlyingType(this Type type)
        {
            if (type.IsGenericType)
            {
                var genType = type.GetGenericTypeDefinition();
                if(genType == typeof(Nullable<>))
                {
                    return genType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        internal static FieldInfo[] GetInstanceFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal static PropertyInfo[] GetInstanceProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        }

        internal static FieldInfo[] GetStaticFields(this Type type)
        {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal static FieldInfo GetStaticField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal static MethodInfo GetPublicInstanceMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

        internal static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            if (!type.IsInterface)
            {
                return type.GetRuntimeMethods();
            }
            else
            {
                return SelfAndBases(type).SelectMany(t => t.GetRuntimeMethods());
            }
        }

        private static IEnumerable<Type> SelfAndBases(Type type)
        {
            List<Type> toReturn = new List<Type> { type };

            if (!type.IsInterface)
            {
                toReturn.AddRange(type.GetAllBases());
            }
            else
            {
                toReturn.AddRange(type.GetInterfaces());
            }

            return toReturn;
        }

        private static Type MakeVectorType(Type elementType, int ignored)
        {
            return TypeHelpers.MakeArrayType(elementType);
        }

        private static Type MakeArrayType(Type elementType, int rank)
        {
            return TypeHelpers.MakeArrayType(elementType, rank);
        }
    }
}
