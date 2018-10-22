// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// A special type of <see cref="ITargetContainer"/> which stores targets (and potentially other containers)
    /// which are specifically keyed either to a particular open generic type or a closed generic built from it.
    /// </summary>
    /// <seealso cref="Rezolver.TargetDictionaryContainer" />
    /// <remarks>You don't typically use this container directly - it is implicitly added to an <see cref="ITargetContainer"/>
    /// when generic types are registered.  Indeed the <see cref="TargetContainer"/> and <see cref="DecoratingTargetContainer"/> both
    /// create instances of this; and the <see cref="EnumerableTargetContainer"/> (understandably) inherits from it.</remarks>
    public class GenericTargetContainer : TargetDictionaryContainer
    {
        private class Caches
        {
            public readonly ConcurrentDictionary<Type, ITarget> FetchCache
                = new ConcurrentDictionary<Type, ITarget>();

            public readonly ConcurrentDictionary<Type, IEnumerable<ITarget>> FetchAllCache
                = new ConcurrentDictionary<Type, IEnumerable<ITarget>>();

            public bool Empty => this.FetchCache.Count == 0 && this.FetchAllCache.Count == 0;
        }

        private Caches _caches = new Caches();

        private void InvalidateCaches()
        {
            this._caches = new Caches();
        }

        /// <summary>
        /// Gets the open generic type definition which is common to all targets and containers within this container.
        /// </summary>
        public Type GenericType { get; }

        private TargetListContainer Targets { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericTargetContainer"/> class.
        /// </summary>
        /// <param name="root">Required.  The root <see cref="ITargetContainer"/> in which the new generic target container
        /// will be registered.</param>
        /// <param name="genericType">Required. The generic type definition that all targets and subcontainers registered
        /// to the new container will have in common.</param>
        public GenericTargetContainer(IRootTargetContainer root, Type genericType)
            : base(root ?? throw new ArgumentNullException(nameof(root)))
        {
            GenericType = genericType ?? throw new ArgumentNullException(nameof(genericType));
            if (!TypeHelpers.IsGenericTypeDefinition(GenericType))
            {
                throw new ArgumentException("type must be a generic type definition", nameof(genericType));
            }

            Targets = new TargetListContainer(Root, genericType);
        }

        /// <summary>
        /// Overrides the <see cref="TargetDictionaryContainer.Register(ITarget, Type)"/> method to support registering
        /// both targets against the open generic type <see cref="GenericType"/> and against specific closed versions of that
        /// open generic type.
        /// </summary>
        /// <param name="target">The target to be registered</param>
        /// <param name="serviceType">Service type against which the registration will be made.  If null, then it
        /// will be assumed to be equal to <see cref="GenericType"/>.  Only types equal to <see cref="GenericType"/> or
        /// generic types whose generic type definition is equal to <see cref="GenericType"/> are supported.</param>
        /// <exception cref="ArgumentException">If <paramref name="serviceType"/> is not equal to <see cref="GenericType"/>
        /// or is not a closed generic type whose generic type definition is <see cref="GenericType"/>.</exception>
        /// <remarks>Notes to overriders: When <paramref name="serviceType"/> is a closed generic type, this function
        /// creates an <see cref="ITargetContainer" /> for that <paramref name="serviceType" />
        /// by calling the protected method <see cref="TargetDictionaryContainer.AutoRegisterContainer(Type)" /> if one doesn't exist
        /// (it calls <see cref="TargetDictionaryContainer.FetchContainer(Type)" /> to check for existence),
        /// and then chains to its <see cref="ITargetContainer.Register(ITarget, Type)" /> method.</remarks>
        public override void Register(ITarget target, Type serviceType = null)
        {
            if (serviceType == null)
            {
                serviceType = target.DeclaredType;
            }

            // if the type we're adding against is equal to this container's generic type definition,
            // then we add it to the collection of targets that are registered specifically against
            // this type.
            if (serviceType == GenericType)
            {
                Targets.Register(target, serviceType);
                InvalidateCaches();
            }
            else
            {
                // the type MUST therefore be a closed generic over the generic type definition,
                // if it's not, then we must throw an exception
                if (!TypeHelpers.IsGenericType(serviceType) || serviceType.GetGenericTypeDefinition() != GenericType)
                {
                    throw new ArgumentException($"Type must be equal to the generic type definition {GenericType} or a closed instance of that type", nameof(serviceType));
                }

                base.Register(target, serviceType);
                InvalidateCaches();
            }
        }

        /// <summary>
        /// Override of the <see cref="TargetDictionaryContainer.RegisterContainer(Type, ITargetContainer)"/> method.
        ///
        /// Doesn't do anything different, except invalidate some internal caches.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public override void RegisterContainer(Type type, ITargetContainer container)
        {
            base.RegisterContainer(type, container);
            InvalidateCaches();
        }

        /// <summary>
        /// Gets the target which can be used to build an instance of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Required.  The type for which a target is to be obtained.  Because of the
        /// restrictions placed on the <see cref="ITarget.DeclaredType"/> of the targets that can actually
        /// be registered into this container, the function will only ever return anything if <paramref name="type"/>
        /// is a closed generic type whose definition equals <see cref="GenericType"/>.</param>
        /// <remarks>Targets which have been registered specifically against the exact closed generic type
        /// represented by <paramref name="type"/> take precedence over any targets which have been registered
        /// against the open generic type <see cref="GenericType"/>.</remarks>
        public override ITarget Fetch(Type type)
        {
            // don't bother checking type validity, just find the container entry with the
            // given type and return the result of its fetch function.
            // If we don't find one - then we return the targets that have been registered against the
            // open generic type definition.
            return this._caches.FetchCache.GetOrAdd(type, t =>
            {
                ITarget baseResult = null;
                var typeSelector = Root.SelectTypes(t);
                foreach (var searchType in typeSelector)
                {
                    if ((baseResult = base.Fetch(searchType)) != null)
                    {
                        if (typeSelector.IsVariantMatch(type, searchType))
                            return VariantMatchTarget.Wrap(baseResult, type, searchType);
                        else
                            return baseResult;
                    }
                }

                // no direct match for any of the search types in our dictionary - so resort to the
                // targets that have been registered directly against the open generic type.
                return Targets.Fetch(t);
            });
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.FetchAll(Type)" />
        /// </summary>
        /// <param name="type">The type whose targets are to be retrieved.</param>
        public override IEnumerable<ITarget> FetchAll(Type type)
        {
            return this._caches.FetchAllCache.GetOrAdd(type, t => FetchAllWorker(t).ToArray());
        }

        private IEnumerable<ITarget> FetchAllWorker(Type type)
        {
            bool matchAll = Root.GetOption(type, Options.FetchAllMatchingGenerics.Default);
            bool foundOne = false;
            var typeSelector = Root.SelectTypes(type);
            // all generics are returned in descending order of specificity
            foreach (var searchType in typeSelector)
            {
                foreach (var result in base.FetchAll(searchType))
                {
                    if (!matchAll && !foundOne)
                    {
                        foundOne = true;
                    }

                    yield return typeSelector.IsVariantMatch(type, searchType) ? VariantMatchTarget.Wrap(result, type, searchType) : result;
                }

                if (!matchAll && foundOne)
                {
                    yield break;
                }
            }

            foreach (var result in Targets.FetchAll(type))
            {
                yield return result;
            }
        }
    }
}
