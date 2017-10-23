using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Events;
using Rezolver.Runtime;

namespace Rezolver
{
    /// <summary>
    /// Serves as a tracker for all types that are registered in 
    /// a Target Container.
    /// 
    /// Under the default configuration, an instance of this is stored as a global option in the target
    /// container which 'listens' for all registrations that are added to it.
    /// 
    /// Other types will also explicitly pull this instance out and manually add other types to it - for example
    /// the <see cref="EnumerableTargetContainer"/> does this as 
    /// </summary>
    internal class KnownTypesIndex : ITargetContainerEventHandler<TargetRegisteredEvent>
    {
        /// <summary>
        /// Any type that's 
        /// </summary>
        public Dictionary<Type, HashSet<Type>> CovariantLookup { get; } = new Dictionary<Type, HashSet<Type>>();
        public HashSet<Type> KnownTypes { get; } = new HashSet<Type>();

        public KnownTypesIndex(ITargetContainer root)
        {
            root.RegisterEventHandler(this);
        }

        void ITargetContainerEventHandler<TargetRegisteredEvent>.Handle(ITargetContainer source, TargetRegisteredEvent e)
        {
            Add(e.ServiceType);
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
                            (ta, tp) => new { typeArg = ta, typeParam = tp })
                        .Select(tap =>
                        // when a type argument is covariant, then we can safely include all its bases and interfaces
                        tap.typeParam.IsCovariantTypeParameter() ? GetAllCompatibleTypes(tap.typeArg) : new[] { tap.typeArg })
                        .Permutate()
                        .Skip(1) // -> First result is always equal to the original type
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

        /// <summary>
        /// Returns an enumerable that contains every type that an instance of <paramref name="type"/>
        /// can be assigned to
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

        public void Add(Type type)
        {
            // Open generic types are not tracked
            if (TypeHelpers.ContainsGenericParameters(type))
                return;

            if (KnownTypes.Add(type))
            {
                foreach (var compatibleType in GetAllCompatibleTypes(type))
                {
                    AddCovariantEntry(type, compatibleType);
                }
            }
        }

        private void AddCovariantEntry(Type type, Type covariantMatch)
        {
            if (!CovariantLookup.TryGetValue(covariantMatch, out var typeList))
                typeList = CovariantLookup[covariantMatch] = new HashSet<Type>();

            typeList.Add(type);
        }

        // note - this only looks in the CovariantLookup lookup
        public IEnumerable<Type> GetKnownTypesCompatibleWith(Type t)
        {
            return CovariantLookup.TryGetValue(t, out var result) ? result : Enumerable.Empty<Type>();
        }
    }
}
