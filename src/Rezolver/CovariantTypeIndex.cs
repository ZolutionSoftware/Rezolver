using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Events;
using Rezolver.Runtime;

namespace Rezolver
{
    /// <summary>
    /// Implementation of <see cref="ICovariantTypeIndex"/> used internally by <see cref="TargetContainer"/>
    /// and <see cref="OverridingTargetContainer"/> in their implementation of the same interface.
    /// </summary>
    public class CovariantTypeIndex : ICovariantTypeIndex
    {
        private readonly Dictionary<Type, OrderedSet<Type>> CovariantLookup = new Dictionary<Type, OrderedSet<Type>>();
        private readonly Dictionary<Type, OrderedSet<Type>> CompatibleLookup = new Dictionary<Type, OrderedSet<Type>>();
        private readonly HashSet<Type> KnownTypes = new HashSet<Type>();

        internal CovariantTypeIndex(IRootTargetContainer root)
        {
            root.TargetRegistered += Root_TargetRegistered;
        }

        private void Root_TargetRegistered(object sender, TargetRegisteredEventArgs e)
        {
            AddKnownType(e.Type);
        }

        /// <summary>
        /// Implementation of <see cref="ICovariantTypeIndex.AddKnownType(Type)"/>
        /// </summary>
        /// <param name="serviceType"></param>
        public void AddKnownType(Type serviceType)
        {
            // Open generic types are not tracked
            if (TypeHelpers.ContainsGenericParameters(serviceType))
                return;

            if (KnownTypes.Add(serviceType))
            {
                if (TypeHelpers.IsArray(serviceType))
                {
                    var elementType = TypeHelpers.GetElementType(serviceType);

                    foreach(var type in GetGenericCovariants(elementType)
                        .Select(t => TypeHelpers.MakeArrayType(t)))
                    {
                        AddCovariantEntry(serviceType, type);
                    }

                    foreach(var type in GetTypeHierarchy(elementType)
                        .Select(t => TypeHelpers.MakeArrayType(t)))
                    {
                        AddCompatibleTypeEntry(serviceType, type);
                    }
                }
                else
                {
                    foreach (var type in GetGenericCovariants(serviceType))
                    {
                        AddCovariantEntry(serviceType, type);
                    }

                    foreach (var type in GetTypeHierarchy(serviceType))
                    {
                        AddCompatibleTypeEntry(serviceType, type);
                    }
                }
            }
        }

        /// <summary>
        /// Implementation of <see cref="ICovariantTypeIndex.GetKnownCovariantTypes(Type)"/>
        /// </summary>
        /// <param name="serviceType"></param>
        public IEnumerable<Type> GetKnownCovariantTypes(Type serviceType)
        {
            return CovariantLookup.TryGetValue(serviceType, out var result) ? result.Reverse() : Enumerable.Empty<Type>();
        }

        /// <summary>
        /// Implementatino of <see cref="ICovariantTypeIndex.GetKnownCompatibleTypes(Type)"/>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetKnownCompatibleTypes(Type serviceType)
        {
            IEnumerable<Type> toReturn = Enumerable.Empty<Type>();
            if (CovariantLookup.TryGetValue(serviceType, out var result))
                toReturn = toReturn.Concat(result.Reverse());
            if (CompatibleLookup.TryGetValue(serviceType, out result))
                toReturn = toReturn.Concat(result.Reverse());
            return toReturn;
        }

        /// <summary>
        /// Produces an enumerable containing all the types that <paramref name="type"/> is compatible with
        /// via generic covariance.  That is - if the type is a generic, then it is processed and each covariant
        /// type argument is processed, with a new generic type created for each base or interface (or similarly covariant
        /// type) baked in.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>If type is a Func&lt;string&gt; then you will get something like:
        /// Func&lt;object&gt;
        /// Func&lt;IEnumerable&lt;char&gt;&gt;
        /// Func&lt;IEnumerable&lt;IComparable&lt;char&gt;&gt;&gt;
        /// 
        /// and so on.</remarks>
        private IEnumerable<Type> GetGenericCovariants(Type type)
        {
            // if this type is a closed generic type whose generic type definition has one or
            // more covariant arguments, then we descend each covariant argument's type hierarchy,
            // constructing a new generic and adding that too.
            if (TypeHelpers.IsGenericType(type) && !TypeHelpers.IsGenericTypeDefinition(type))
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                // get the generic type arguments and the parameters they belong to.
                return TypeHelpers.GetGenericArguments(type)
                        .Zip(
                            TypeHelpers.GetGenericArguments(genericTypeDef),
                            (ta, tp) => new { typeArg = ta, typeParam = tp }
                        )
                        .Select(tap =>
                            // when a type argument is covariant, then we can safely include all its bases and interfaces
                            tap.typeParam.IsCovariantTypeParameter() ? GetAllCompatibleTypes(tap.typeArg) : new[] { tap.typeArg }
                        )
                        .Permutate()
                        .Select(typeArgs =>
                        {
                            try
                            {
                                return genericTypeDef.MakeGenericType(typeArgs.ToArray());
                            }
                            catch (ArgumentException)
                            {
                                // Gulping ArgumentException here in case of things like 
                                // generic constraints being violated.  There's no sense in
                                // reinventing that wheel in order to avoid possibly creating
                                // an invalid type - just catch the exception and move on.
                                return null;
                            }
                        })
                        .Where(t => t != null);
            }
            return Enumerable.Empty<Type>();
        }
        private IEnumerable<Type> GetTypeHierarchy(Type type)
        {
            var toReturn = Enumerable.Empty<Type>();
            if (TypeHelpers.IsClass(type))
                toReturn = toReturn.Concat(type.GetAllBases());

            toReturn = toReturn.Concat(TypeHelpers.GetInterfaces(type));
            return toReturn.OrderBy(t => t, DescendingTypeOrder.Instance);
        }
        /// <summary>
        /// Returns an enumerable that contains every type that an instance of <paramref name="type"/>
        /// can be assigned to, taking into account bases, interfaces and, in the case of generics,
        /// variants of the same generic which use more derived types for any covariant generic parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetAllCompatibleTypes(Type type)
        {
            IEnumerable<Type> toReturn = Enumerable.Empty<Type>();

            if (TypeHelpers.IsArray(type))
            {
                toReturn = toReturn.Concat(
                    GetAllCompatibleTypes(TypeHelpers.GetElementType(type))
                    .Select(t => TypeHelpers.MakeArrayType(type)));
            }

            // if the type is generic, then this will generate any covariant permutations
            // of its type arguments.
            toReturn = toReturn.Concat(GetGenericCovariants(type));
            toReturn = toReturn.Concat(GetTypeHierarchy(type).SelectMany(t => new[] { t }.Concat(GetGenericCovariants(t))));

            return toReturn;
        }

        private void AddCovariantEntry(Type type, Type covariantMatch)
        {
            if (!CovariantLookup.TryGetValue(covariantMatch, out var typeList))
                typeList = CovariantLookup[covariantMatch] = new OrderedSet<Type>();

            typeList.Add(type);
        }

        private void AddCompatibleTypeEntry(Type type, Type compatible)
        {
            if (!CompatibleLookup.TryGetValue(compatible, out var typeList))
                typeList = CompatibleLookup[compatible] = new OrderedSet<Type>();

            typeList.Add(type);
        }
    }
}
