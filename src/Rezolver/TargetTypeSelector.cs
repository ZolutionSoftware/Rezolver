// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Runtime;

namespace Rezolver
{
    /// <summary>
    /// Given a type, this produces an enumerable of all types to be sought from an <see cref="ITargetContainer"/>
    /// whose targets might produce a compatible instance.
    ///
    /// In particular, this class is responsible for handling generic type matching, including variance.
    /// </summary>
    /// <remarks>It's *highly unlikely* that you will ever need to use this type directly in an application.
    ///
    /// It's public because it could be useful to developers of components which extend Rezolver.
    ///
    /// Internally, the <see cref="GenericTargetContainer"/> uses this exclusively to perform searches for
    /// compatible target types if a requested type is generic.</remarks>
    public partial class TargetTypeSelector : IEnumerable<Type>
    {
        // NOTE TO SELF!
        // DO NOT USE OPTIONS IN THIS CLASS BECAUSE THE OPTIONS CODE TRIGGERS
        // ANOTHER CALL TO TARGETTYPESELECTOR WHICH ALWAYS CAUSES A STACKOVERFLOWEXCEPTION!

        /// <summary>
        /// The type for which compatible targets are sought.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// The root-most <see cref="ITargetContainer"/> containing registrations to be sought,
        /// and the source of any configuration options.
        /// </summary>
        public IRootTargetContainer RootTargets { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TargetTypeSelector"/> type for the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which a list of search types is to be produced.</param>
        /// <param name="rootTargets">The root target container</param>
        public TargetTypeSelector(Type type, IRootTargetContainer rootTargets = null)
        {
            this.Type = type;
            this.RootTargets = rootTargets;
        }

        private IEnumerable<Type> Run(TargetTypeSelectorParams search)
        {
            // for IFoo<IEnumerable<IBar<in T>>>, this should return something like
            // IFoo<IEnumerable<IBar<T>>>,
            // IFoo<IEnumerable<IBar<T[Base0..n]>>>,    //foreach base and interface of T if contravariant
            // IFoo<IEnumerable<IBar<>>,
            // IFoo<IEnumerable<>
            // IFoo<>

            // using an iterator method is not the best for performance, but fetching type
            // registrations from a container builder is an operation that, so long as a caching
            // resolver is used, shouldn't be repeated often

            List<Type> toReturn = new List<Type>(20)
            {
                search.Type
            };

            // these get added after all other bases and interfaces have been added.
            List<Type> explicitlyAddedBases = new List<Type>();

            if (TypeHelpers.IsGenericType(search.Type) && !TypeHelpers.IsGenericTypeDefinition(search.Type))
            {
                // now return any covariant matches if applicable
                // NOTE - if the search is for a type parameter, then we don't perform this operation
                // as we're only interested in materialising covariant types which we know *should* materialise
                // at least one target match.
                if (search.TypeParameter == null &&
                    !TypeHelpers.IsValueType(search.Type) &&
                    this.RootTargets != null)
                {
                    toReturn.AddRange(this.RootTargets.GetKnownCovariantTypes(search.Type));
                }

                // for every generic type, there is at least two versions - the closed and the open
                // when you consider, also, that a generic parameter might also be a generic, with multiple
                // versions - you can see that things can get icky.
                var argSearches = TypeHelpers.GetGenericArguments(search.Type).Zip(
                TypeHelpers.GetGenericArguments(search.Type.GetGenericTypeDefinition()),
                (arg, param) => new TargetTypeSelectorParams(
                    arg,
                    param,
                    search)
                );

                var typeParamSearchLists = argSearches.Select(t => this.Run(t).ToArray()).ToArray();
                var genericType = search.Type.GetGenericTypeDefinition();

                // Note: the first result will be equal to search.Type, hence Skip(1)
                foreach (var combination in typeParamSearchLists.Permutate().Skip(1))
                {
                    var compatibleType = genericType.MakeGenericType(combination.ToArray());
                    toReturn.Add(compatibleType);
                    if (search.TypeParameter == null &&
                        !TypeHelpers.IsValueType(search.Type) &&
                        this.RootTargets != null)
                    {
                        toReturn.AddRange(this.RootTargets.GetKnownCovariantTypes(compatibleType));
                    }
                }

                toReturn.Add(genericType);
            }

            bool isArray = TypeHelpers.IsArray(search.Type);
            Type arrayElemType = null;
            bool isValueTypeArray = false;
            bool addElemTypeInterfaces = false;
            bool addArrayBases = false;
            List<Type> allArrayTypes = new List<Type>();

            if (isArray)
            {
                allArrayTypes.Add(search.Type);
                arrayElemType = TypeHelpers.GetElementType(search.Type);
                isValueTypeArray = TypeHelpers.IsValueType(arrayElemType);
                if (!isValueTypeArray)
                {
                    addElemTypeInterfaces = TypeHelpers.IsInterface(arrayElemType);
                    addArrayBases = !addElemTypeInterfaces;
                }
            }

            if (search.EnableVariance)
            {
                if (search.TypeParameterIsContravariant &&
                    (search.Contravariance & Contravariance.BasesAndInterfaces) != Contravariance.None)
                {
                    // note - when spawning child searches, this search's parent is passed as the parent
                    // to the newly created search.  This ensures that the contravariant search direction
                    // is calculated correctly

                    if ((search.Contravariance & Contravariance.Bases) == Contravariance.Bases)
                    {
                        if (isArray)
                        {
                            explicitlyAddedBases.Add(typeof(Array));
                            explicitlyAddedBases.Add(typeof(object));
                            // Arrays are weird.  Given an instance i of type A[], where A is a reference or interface type,
                            // then i can be implicitly converted to an array of any base or interface ('supertype') of A; and
                            // can also be implicitly converted to any interface or base of an array of that supertype.
                            // So, given Foo : IFoo
                            // We can have IFoo[] a = new Foo[10];
                            // We can also have IList<IFoo> a = new Foo[10];
                            // Because IList<T> is an interface of IFoo[].
                            // The same also holds true for any contravariant type argument, so:
                            // Action<IList<IFoo>> a = new Action<Foo[]>(foos => <<blah>>) is allowed.
                            if (addArrayBases)
                            {
                                allArrayTypes.AddRange(search.Type.GetBaseArrayTypes());
                            }
                            else if (addElemTypeInterfaces)
                            {
                                allArrayTypes.AddRange(search.Type.GetInterfaceArrayTypes());
                            }

                            // loop through the bases, constructing array types for each and adding them
                            // note we skip the first because it'll be the search type.
                            toReturn.AddRange(allArrayTypes.Skip(1)
                                .SelectMany(tt => this.Run(new TargetTypeSelectorParams(tt, search.TypeParameter, search.Parent, Contravariance.Bases)))
                                .Where(tt => !explicitlyAddedBases.Contains(tt)));
                        }

                        // if it's a class then iterate the bases
                        if (!TypeHelpers.IsInterface(search.Type)
                            && !TypeHelpers.IsValueType(search.Type)
                            && search.Type != typeof(object))
                        {
                            if (!explicitlyAddedBases.Contains(typeof(object)))
                            {
                                explicitlyAddedBases.Add(typeof(object));
                            }

                            // note - disable interfaces when recursing into the base
                            // note also - ignore 'object'
                            toReturn.AddRange(
                                this.Run(new TargetTypeSelectorParams(TypeHelpers.BaseType(search.Type), search.TypeParameter, search.Parent, Contravariance.Bases))
                                .Where(tt => !explicitlyAddedBases.Contains(tt)));
                        }
                    }

                    if ((search.Contravariance & Contravariance.Interfaces) == Contravariance.Interfaces)
                    {
                        // don't check interfaces when type is a value type
                        if (!TypeHelpers.IsValueType(search.Type))
                        {
                            if (isArray)
                            {
                                // have to include all the interfaces of all the array types that are compatible per array covariance
                                toReturn.AddRange(allArrayTypes
                                    .SelectMany(t => TypeHelpers.GetInterfaces(t)
                                    .SelectMany(tt => this.Run(new TargetTypeSelectorParams(tt, search.TypeParameter, search.Parent, Contravariance.Bases))))
                                    .OrderBy(t => t, DescendingTypeOrder.Instance)
                                    .Where(tt => !explicitlyAddedBases.Contains(tt)));
                            }
                            else
                            {
                                toReturn.AddRange(TypeHelpers.GetInterfaces(search.Type)
                                    .SelectMany(t => this.Run(new TargetTypeSelectorParams(t, search.TypeParameter, search.Parent, Contravariance.Bases)))
                                    .Where(tt => !explicitlyAddedBases.Contains(tt)));
                            }
                        }
                    }

                    toReturn.AddRange(explicitlyAddedBases);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Type> GetEnumerator()
        {
            return this.Run(new TargetTypeSelectorParams(this.Type, this)).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
