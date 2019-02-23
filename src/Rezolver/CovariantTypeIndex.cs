// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private class CachedTargetTypeSelector
        {
            public long Version;
            public TargetTypeSelector Selector;
        }

        private IRootTargetContainer _root;
        private readonly Dictionary<Type, OrderedSet<Type>> CovariantLookup = new Dictionary<Type, OrderedSet<Type>>();
        private readonly Dictionary<Type, OrderedSet<Type>> CompatibleLookup = new Dictionary<Type, OrderedSet<Type>>();
        private readonly HashSet<Type> KnownTypes = new HashSet<Type>();
        private readonly ConcurrentDictionary<Type, CachedTargetTypeSelector> _cachedSearches = new ConcurrentDictionary<Type, CachedTargetTypeSelector>();
        private long _version = 0;

        internal CovariantTypeIndex(IRootTargetContainer root)
        {
            _root = root;
            _root.TargetRegistered += Root_TargetRegistered;
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
            if (serviceType.ContainsGenericParameters)
            {
                return;
            }
            Interlocked.Increment(ref _version);
            if (this.KnownTypes.Add(serviceType))
            {
                Stack<Type> derivedTypeStack = new Stack<Type>(new[] { serviceType });
                if (serviceType.IsArray)
                {
                    var elementType = serviceType.GetElementType();

                    foreach (var type in GetGenericCovariants(elementType, derivedTypeStack)
                        .Select(t => TypeHelpers.MakeArrayType(t)))
                    {
                        AddCovariantEntry(serviceType, type);
                    }

                    foreach (var type in GetTypeHierarchy(elementType)
                        .Select(t => TypeHelpers.MakeArrayType(t)))
                    {
                        AddCompatibleTypeEntry(serviceType, type);
                    }
                }
                else
                {
                    foreach (var type in GetGenericCovariants(serviceType, derivedTypeStack))
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
            return this.CovariantLookup.TryGetValue(serviceType, out var result) ? result.Reverse() : Enumerable.Empty<Type>();
        }

        /// <summary>
        /// Implementatino of <see cref="ICovariantTypeIndex.GetKnownCompatibleTypes(Type)"/>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetKnownCompatibleTypes(Type serviceType)
        {
            IEnumerable<Type> toReturn = Enumerable.Empty<Type>();
            if (this.CovariantLookup.TryGetValue(serviceType, out var result))
            {
                toReturn = toReturn.Concat(result.Reverse());
            }

            if (this.CompatibleLookup.TryGetValue(serviceType, out result))
            {
                toReturn = toReturn.Concat(result.Reverse());
            }

            return toReturn;
        }

        /// <summary>
        /// Produces an enumerable containing all the types that <paramref name="type"/> is compatible with
        /// via generic covariance.  That is - if the type is a generic, then it is processed and each covariant
        /// type argument is processed, with a new generic type created for each base or interface (or similarly covariant
        /// type) baked in.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="derivedTypeStack">A stack of derived types through which the recursion has passed.
        ///
        /// Used to prevent recursion into a derived type through a generic type argument supplied to a base
        /// or interface of that same type.</param>
        /// <returns></returns>
        /// <remarks>If type is a Func&lt;string&gt; then you will get something like:
        /// Func&lt;object&gt;
        /// Func&lt;IEnumerable&lt;char&gt;&gt;
        /// Func&lt;IEnumerable&lt;IComparable&lt;char&gt;&gt;&gt;
        ///
        /// and so on.</remarks>
        private IEnumerable<Type> GetGenericCovariants(Type type, Stack<Type> derivedTypeStack)
        {
            // if this type is a closed generic type whose generic type definition has one or
            // more covariant arguments, then we descend each covariant argument's type hierarchy,
            // constructing a new generic and adding that too.
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                // get the generic type arguments and the parameters they belong to.
                return type.GetGenericArguments()
                        .Zip(
                            genericTypeDef.GetGenericArguments(),
                            (ta, tp) => (typeArg: ta, typeParam: tp)
                        )
                        .Select(tap =>
                            // when a type argument is passed to a covariant parameter, then we can
                            // safely include all its bases and interfaces so long as it's not a type
                            // that is a sub type (or implementer)
                            tap.typeParam.IsCovariantTypeParameter() && !derivedTypeStack.Contains(tap.typeArg) ? GetAllCompatibleTypes(tap.typeArg, derivedTypeStack) : new[] { tap.typeArg }
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
                        .Where(t => t != null && t != type);
            }

            return Enumerable.Empty<Type>();
        }

        private IEnumerable<Type> GetTypeHierarchy(Type type)
        {
            var toReturn = Enumerable.Empty<Type>();
            if (type.IsClass)
            {
                toReturn = toReturn.Concat(type.GetAllBases());
            }

            toReturn = toReturn.Concat(type.GetInterfaces());
            return toReturn.OrderBy(t => t, DescendingTypeOrder.Instance);
        }

        /// <summary>
        /// Returns an enumerable that contains every type that an instance of <paramref name="type"/>
        /// can be assigned to, taking into account bases, interfaces and, in the case of generics,
        /// variants of the same generic which use more derived types for any covariant generic parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="derivedTypeStack">A state stack which is used to prevent descending into the type
        /// hierarchies of type arguments to covariant type parameters which are equal to a type that is
        /// inheriting/implementing the generic.</param>
        /// <returns></returns>
        /// <remarks><code>class MyClass : IEnumerable&lt;MyClass&gt;</code> is an example of a type for which
        /// the <paramref name="derivedTypeStack"/> parameter is required.</remarks>
        private IEnumerable<Type> GetAllCompatibleTypes(Type type, Stack<Type> derivedTypeStack)
        {
            List<Type> toReturn = new List<Type>();

            if (type.IsArray)
            {
                foreach (var t in GetAllCompatibleTypes(type.GetElementType(), derivedTypeStack))
                {
                    toReturn.Add(TypeHelpers.MakeArrayType(t));
                }
            }

            // if the type is generic, then this will generate any covariant
            // permutations of its type arguments.
            foreach (var t in GetGenericCovariants(type, derivedTypeStack)) // <-- HERE pass the stack and prevent covariants being generated when type arg is equal to one in the stack
            {
                toReturn.Add(t);
            }

            // HERE - Need to push the source type into a stack (which might be passed in) which will prevent us from using any
            // derived type as a type argument to any covariant type parameters.
            derivedTypeStack.Push(type);
            foreach (var t in GetTypeHierarchy(type))
            {
                toReturn.Add(t);
                derivedTypeStack.Push(t);
                foreach (var tco in GetGenericCovariants(t, derivedTypeStack))
                {
                    toReturn.Add(tco);
                }

                derivedTypeStack.Pop();
            }

            derivedTypeStack.Pop();

            return toReturn.AsReadOnly();
        }

        private void AddCovariantEntry(Type type, Type covariantMatch)
        {
            if (!this.CovariantLookup.TryGetValue(covariantMatch, out var typeList))
            {
                typeList = this.CovariantLookup[covariantMatch] = new OrderedSet<Type>();
            }

            typeList.Add(type);
        }

        private void AddCompatibleTypeEntry(Type type, Type compatible)
        {
            if (!this.CompatibleLookup.TryGetValue(compatible, out var typeList))
            {
                typeList = this.CompatibleLookup[compatible] = new OrderedSet<Type>();
            }

            typeList.Add(type);
        }
        public TargetTypeSelector SelectTypes(Type type)
        {
            // TODO: implement a 'stability' calculator for this function which disables caching until the _version
            // local has not changed for a certain number of successive calls. 
            // -- UPDATE to above.  I tried it - it made no difference.

            // this function is complicated by the fact that searching and registering types are asymmetrically parallel.
            // in theory most applications will register first and then start searching.  However, the searching part is parallelised
            // equally, if registrations continue whilst searching is being performed, then we'd have a problem.  So this is written to
            // ensure that the cached type selectors stay up to date as extra registrations are made.
            var result = _cachedSearches.GetOrAdd(type,
                t => new CachedTargetTypeSelector() { Version = Interlocked.Read(ref _version), Selector = new TargetTypeSelector(type, _root) });
            long resultVersion = Interlocked.Read(ref result.Version);
            long currentVersion = Interlocked.Read(ref _version);
            long oldResultVersion = resultVersion;
            // it's fresh enough
            if (resultVersion == currentVersion)
                return result.Selector;

            while (true)
            {
                resultVersion = Interlocked.Read(ref result.Version);
                currentVersion = Interlocked.Read(ref _version);
                var newSelector = new TargetTypeSelector(type, _root);
                result.Selector = newSelector;
                if (Interlocked.CompareExchange(ref result.Version, currentVersion, resultVersion) == resultVersion)
                    break;
            }

            return result.Selector;
        }
    }
}
