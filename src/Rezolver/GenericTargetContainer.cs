// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
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
    /// when generic types are registered.  Indeed the <see cref="TargetContainer"/> and <see cref="DecoratingTargetContainer"/> booth
    /// create instances of this; and the <see cref="EnumerableTargetContainer"/> (understandably) inherits from it.</remarks>
    public class GenericTargetContainer : TargetDictionaryContainer
    {
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
        public GenericTargetContainer(ITargetContainer root, Type genericType)
            : base(root ?? throw new ArgumentNullException(nameof(root)))
        {
            GenericType = genericType ?? throw new ArgumentNullException(nameof(genericType));
            if (!TypeHelpers.IsGenericTypeDefinition(GenericType)) throw new ArgumentException("type must be a generic type definition", nameof(genericType));

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
        /// by calling the protected method <see cref="TargetDictionaryContainer.CreateContainer(Type, ITarget)" /> if one doesn't exist
        /// (it calls <see cref="TargetDictionaryContainer.FetchContainer(Type)" /> to check for existence),
        /// and then chains to its <see cref="ITargetContainer.Register(ITarget, Type)" /> method.</remarks>
        public override void Register(ITarget target, Type serviceType = null)
        {
            if (serviceType == null) serviceType = target.DeclaredType;

            //if the type we're adding against is equal to this container's generic type definition,
            //then we add it to the collection of targets that are registered specifically against
            //this type.
            if (serviceType == GenericType)
                Targets.Register(target, serviceType);
            else
            {
                //the type MUST therefore be a closed generic over the generic type definition,
                //if it's not, then we must throw an exception
                if (!TypeHelpers.IsGenericType(serviceType) || serviceType.GetGenericTypeDefinition() != GenericType)
                    throw new ArgumentException($"Type must be equal to the generic type definition { GenericType } or a closed instance of that type", nameof(serviceType));
                base.Register(target, serviceType);
            }
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
            //don't bother checking type validity, just find the container entry with the 
            //given type and return the result of its fetch function.
            //If we don't find one - then we return the targets that have been registered against the 
            //open generic type definition.
            ITarget baseResult = null;
            foreach (var searchType in DeriveGenericTypeSearchList(type))
            {
                if ((baseResult = base.Fetch(searchType)) != null)
                    return baseResult;
            }

            //no direct match for any of the search types in our dictionary - so resort to the 
            //targets that have been registered directly against the open generic type.
            return Targets.Fetch(type);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.FetchAll(Type)" />
        /// </summary>
        /// <param name="type">The type whose targets are to be retrieved.</param>
        public override IEnumerable<ITarget> FetchAll(Type type)
        {
            bool matchAll = Root.GetOption(Options.FetchAllMatchingGenerics.Default);
            bool foundOne = false;

            //all generics are returned in descending order of specificity
            foreach (var searchType in DeriveGenericTypeSearchList(type))
            {
                foreach (var result in base.FetchAll(searchType))
                {
                    if (!matchAll && !foundOne) foundOne = true;
                    yield return result;
                }
                if (!matchAll && foundOne) yield break;
            }

            foreach (var result in Targets.FetchAll(type))
            {
                yield return result;
            }
        }

        private class GenericTypeSearch
        {
            public GenericTypeSearch Parent { get; }
            public Type Type { get; }
            public Type TypeParameter { get; }

            public int ConcreteTypeHierarchyDirection { get; }

            public GenericTypeSearch(Type type, Type typeParameter = null, GenericTypeSearch parent = null)
            {
                Parent = parent;
                Type = type;
                TypeParameter = typeParameter;

                // determine variance, and whether concrete bases or derived types should be 
                // sought.  This is a combination of whether the current 
                if (TypeParameterIsCovariant)
                {
                    // if(Parent == null || Parent.)
#error open targetbasetests.cs to see a whole bunch of examples.
#error what I'm currently thinking is that contra parameters flip the search direction; but co parameters cause a derived search, but never change any pre-existing search direction.
                }
            }

            public static implicit operator GenericTypeSearch(Type t)
            {
                return new GenericTypeSearch(t);
            }

            public bool TypeParameterIsContravariant
            {
                get
                {
                    return TypeParameter == null ? false :
                        (TypeHelpers.GetGenericParameterAttributes(TypeParameter) & GenericParameterAttributes.Contravariant)
                        == GenericParameterAttributes.Contravariant;
                }
            }

            public bool TypeParameterIsCovariant
            {
                get
                {
                    return TypeParameter == null ? false :
                        (TypeHelpers.GetGenericParameterAttributes(TypeParameter) & GenericParameterAttributes.Covariant)
                        == GenericParameterAttributes.Covariant;
                }
            }

            /// <summary>
            /// True if the <see cref="TypeParameter"/> is not null and is either covariant
            /// or contravariant
            /// </summary>
            public bool TypeParameterIsVariant
            {
                get
                {
                    return TypeParameter == null ? false :
                        (TypeHelpers.GetGenericParameterAttributes(TypeParameter) & GenericParameterAttributes.VarianceMask)
                        == GenericParameterAttributes.VarianceMask;
                }
            }
        }

        private IEnumerable<Type> DeriveGenericTypeSearchList(GenericTypeSearch search)
        {
            //for IFoo<IEnumerable<Nullable<T>>>, this should return something like
            //IFoo<IEnumerable<Nullable<T>>>, 
            //IFoo<IEnumerable<Nullable<>>>, 
            //IFoo<IEnumerable<>>,
            //IFoo<>

            //using an iterator method is not the best for performance, but fetching type
            //registrations from a container builder is an operation that, so long as a caching
            //resolver is used, shouldn't be repeated often.
            
            if (!TypeHelpers.IsGenericType(search.Type) || TypeHelpers.IsGenericTypeDefinition(search.Type))
            {
                yield return search.Type;

                if(search.ConcreteTypeHierarchyDirection == -1)
                {
                    //if it's a class then iterate the bases
                    if (!TypeHelpers.IsInterface(search.Type))
                    {
                        foreach(var baseClass in TypeHelpers.GetAllBases(search.Type))
                        {
                            yield return baseClass;
                        }
                    }
                }
                yield break;
            }

            //for every generic type, there is at least two versions - the closed and the open
            //when you consider, also, that a generic parameter might also be a generic, with multiple
            //versions - you can see that things can get icky.  
            var typeArgs = TypeHelpers.GetGenericArguments(search.Type).Zip(TypeHelpers.GetGenericArguments(search.Type.GetGenericTypeDefinition()), 
                (arg, param) => new GenericTypeSearch { Parent = search, Type = arg, TypeParameter = param });
            var typeParamSearchLists = typeArgs.Select(t => DeriveGenericTypeSearchList(t).ToArray()).ToArray();
            var genericType = search.Type.GetGenericTypeDefinition();

            foreach (var combination in CartesianProduct(typeParamSearchLists))
            {
                yield return genericType.MakeGenericType(combination.ToArray());
            }
            yield return genericType;
        }

        static IEnumerable<IEnumerable<T>> CartesianProduct<T>
        (IEnumerable<IEnumerable<T>> sequences)
        {
            //thank you Eric Lippert...
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }

    }
}
