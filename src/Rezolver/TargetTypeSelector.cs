using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Given a type, this produces an enumerable of all types to be sought from an <see cref="ITargetContainer"/>
    /// whose targets might produce a compatible instance.
    /// 
    /// In particular, this class is reponsible for handling generic type matching, including variance.
    /// </summary>
    /// <remarks>It's not likely that you will use this type directly.</remarks>
    public class TargetTypeSelector : IEnumerable<Type>
    {
        /// <summary>
        /// Generates a reasonable ordering of types where:
        /// Derived types are sorted before bases
        /// Interface implementations are sorted before the interfaces
        /// Fully-closed generics are sorted earlier than any containing generic parameters (or which are open).
        /// Generics are also sorted before non-generics.  It's not a general-purpose type sort: it's specifically
        /// useful for our TargetTypeSelector.
        /// </summary>
        private class DescendingTypeOrder : IComparer<Type>
        {
            public static readonly DescendingTypeOrder Instance = new DescendingTypeOrder();

            private DescendingTypeOrder() { }

            public int Compare(Type x, Type y)
            {
                if (x == y)
                    return 0;

                // if x is a base of y then it's greater than y (and vice-versa)
                // if x is an interface of y then it's greater than y (and vice-versa)

                if (TypeHelpers.IsClass(x))
                {
                    if (TypeHelpers.IsClass(y))
                    {
                        if (TypeHelpers.IsSubclassOf(x, y))
                            return -1;
                        else if (TypeHelpers.IsSubclassOf(y, x))
                            return 1;
                    }
                    else if (TypeHelpers.IsInterface(y))
                    {
                        // y is an interface of x
                        if (TypeHelpers.IsAssignableFrom(y, x))
                            return -1;
                    }
                }
                else if (TypeHelpers.IsInterface(x))
                {
                    if (TypeHelpers.IsClass(y))
                    {
                        // x is an interface of y
                        if (TypeHelpers.IsAssignableFrom(x, y))
                            return 1;
                    }
                    else if (TypeHelpers.IsInterface(y))
                    {
                        if (TypeHelpers.GetInterfaces(x).Contains(y))
                            return -1;
                        else if (TypeHelpers.GetInterfaces(y).Contains(x))
                            return 1;
                    }
                }

                if (TypeHelpers.IsGenericType(x))
                {
                    if (TypeHelpers.IsGenericType(y))
                    {
                        if (x.IsConstructedGenericType)
                        {
                            if(y.IsConstructedGenericType)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            if (y.IsConstructedGenericType)
                            {
                                return -1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if(TypeHelpers.IsGenericType(y)) //place nongeneric types after generic types.
                    {
                        return 1;
                    }

                }

                return 0;
            }
        }

        [Flags]
        private enum Contravariance
        {
            None = 0,
            Bases = 1,
            Interfaces = 2,
            BasesAndInterfaces = Bases | Interfaces,
            Derived = 4
        }
        private class SearchParams
        {
            public SearchParams Parent { get; }
            public Type Type { get; }
            public Type TypeParameter { get; }

            public bool EnableVariance { get; }

            public Contravariance Contravariance { get; }

            public IEnumerable<Type> TypeParameterChain
            {
                get
                {
                    var current = this;
                    Type last = null;
                    while (current != null && current.TypeParameter != null)
                    {
                        if (current.TypeParameter != last)
                        {
                            yield return current.TypeParameter;
                        }
                        current = current.Parent;
                    }
                }
            }

            public SearchParams(Type type,
                Type typeParameter = null,
                SearchParams parent = null,
                Contravariance? contravariantSearchType = null)
            {
                Parent = parent;
                Type = type;
                TypeParameter = typeParameter;

                // We enable variance if we have no parent
                // Or if we have a variant type parameter and
                // the parent hasn't disabled variance.
                EnableVariance = parent == null ||
                    (TypeParameterIsVariant && Parent.EnableVariance);

                if (EnableVariance)
                {
                    if (contravariantSearchType != null)
                        Contravariance = contravariantSearchType.Value;
                    else
                    {
                        // if the parent has its contravariance search set to None, we inherit that
                        // and move on.
                        if (Parent?.Contravariance == Contravariance.None)
                            Contravariance = Contravariance.None;
                        else
                        {
                            var numContras = TypeParameterChain.Count(t => t.IsContravariantTypeParameter());
                            if (numContras <= 1 || (numContras % 2) == 1)
                                Contravariance = Contravariance.BasesAndInterfaces;
                            else
                                Contravariance = Contravariance.Derived;
                        }
                    }
                }
            }

            public static implicit operator SearchParams(Type t)
            {
                return new SearchParams(t);
            }

            public bool TypeParameterIsVariant
            {
                get
                {
                    return TypeParameter?.IsVariantTypeParameter() ?? false;
                }
            }

            public bool TypeParameterIsContravariant
            {
                get
                {
                    return TypeParameter?.IsContravariantTypeParameter() ?? false;
                }
            }

            public bool TypeParameterIsCovariant
            {
                get
                {
                    return TypeParameter?.IsCovariantTypeParameter() ?? false;
                }
            }

            public override string ToString()
            {
                if (TypeParameter != null)
                    return $"{TypeParameter.Name}(#{TypeParameter.GenericParameterPosition}) = { Type.CSharpLikeTypeName() } for { Parent }";
                else
                    return Type.CSharpLikeTypeName();
            }
        }

        private IEnumerable<Type> Run(SearchParams search)
        {
            //for IFoo<IEnumerable<IBar<in T>>>, this should return something like
            //IFoo<IEnumerable<IBar<T>>>, 
            //IFoo<IEnumerable<IBar<T[Base0..n]>>>,    //foreach base and of T if contravariant
            //IFoo<IEnumerable<IBar<>>,
            //IFoo<IEnumerable<>
            //IFoo<>

            //using an iterator method is not the best for performance, but fetching type
            //registrations from a container builder is an operation that, so long as a caching
            //resolver is used, shouldn't be repeated often
            yield return search.Type;

            // these get added after all other bases and interfaces have been added.
            List<Type> explicitlyAddedBases = new List<Type>();

            if (TypeHelpers.IsGenericType(search.Type) && !TypeHelpers.IsGenericTypeDefinition(search.Type))
            {
                bool enableContravariance = true;
                if (_rootContainer != null)
                    enableContravariance = _rootContainer.GetOption(search.Type, Options.EnableContravariance.Default);

                //for every generic type, there is at least two versions - the closed and the open
                //when you consider, also, that a generic parameter might also be a generic, with multiple
                //versions - you can see that things can get icky.  
                var argSearches = TypeHelpers.GetGenericArguments(search.Type).Zip(
                    TypeHelpers.GetGenericArguments(search.Type.GetGenericTypeDefinition()),
                    (arg, param) => new SearchParams(
                        arg,
                        param,
                        search,
                        !enableContravariance ? Contravariance.None : (Contravariance?)null)
                    );
                var typeParamSearchLists = argSearches.Select(t => Run(t).ToArray()).ToArray();
                var genericType = search.Type.GetGenericTypeDefinition();

                // Note: the first result will be equal to search.Type, hence Skip(1)
                foreach (var combination in Permutate(typeParamSearchLists).Skip(1))
                {
                    yield return genericType.MakeGenericType(combination.ToArray());
                }

                yield return genericType;
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

            if (search.EnableVariance
                && search.TypeParameterIsContravariant
                && (search.Contravariance & Contravariance.BasesAndInterfaces) != Contravariance.None)
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
                        foreach (var t in allArrayTypes.Skip(1)
                            .SelectMany(tt => Run(new SearchParams(tt, search.TypeParameter, search.Parent, Contravariance.Bases)))
                            .Where(tt => !explicitlyAddedBases.Contains(tt)))
                        {
                            yield return t;
                        }
                    }

                    //if it's a class then iterate the bases
                    if (!TypeHelpers.IsInterface(search.Type)
                        && !TypeHelpers.IsValueType(search.Type)
                        && search.Type != typeof(object))
                    {
                        if(!explicitlyAddedBases.Contains(typeof(object)))
                            explicitlyAddedBases.Add(typeof(object));
                        // note - disable interfaces when recursing into the base
                        // note also - ignore 'object'
                        foreach (var t in
                            Run(new SearchParams(TypeHelpers.BaseType(search.Type), search.TypeParameter, search.Parent, Contravariance.Bases))
                            .Where(tt => !explicitlyAddedBases.Contains(tt)))
                        {
                            // note above: Contravariant searching is set to 'Bases' only because only the top-level call deals with interfaces - 
                            // and it deals with them LAST.
                            yield return t;
                        }
                    }
                }

                if ((search.Contravariance & Contravariance.Interfaces) == Contravariance.Interfaces)
                {
                    // don't check interfaces when type is a value type
                    if (!TypeHelpers.IsValueType(search.Type))
                    {
                        if (isArray)
                        {
                            //have to include all the interfaces of all the array types that are compatible per array covariance
                            foreach (var t in allArrayTypes
                                .SelectMany(t => TypeHelpers.GetInterfaces(t)
                                .SelectMany(tt => Run(new SearchParams(tt, search.TypeParameter, search.Parent, Contravariance.Bases))))
                                .OrderBy(t => t, DescendingTypeOrder.Instance)
                                .Where(tt => !explicitlyAddedBases.Contains(tt)))
                            {
                                yield return t;
                            }

                        }
                        else
                        {
                            foreach (var i in TypeHelpers.GetInterfaces(search.Type)
                                .SelectMany(t => Run(new SearchParams(t, search.TypeParameter, search.Parent, Contravariance.Bases)))
                                .Where(tt => !explicitlyAddedBases.Contains(tt)))
                            {
                                yield return i;
                            }
                        }
                    }
                }
                foreach (var t in explicitlyAddedBases)
                    yield return t;
            }


        }

        static IEnumerable<IEnumerable<T>> Permutate<T>
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



        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Type> GetEnumerator()
        {
            return Run(_type).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly Type _type;
        private readonly ITargetContainer _rootContainer;

        /// <summary>
        /// Creates a new instance of the <see cref="TargetTypeSelector"/> type for the given
        /// <paramref name="type"/> - with options read from the given <paramref name="rootContainer"/>.
        /// </summary>
        /// <param name="type">The type for which a list of search types is to be produced.</param>
        /// <param name="rootContainer"></param>
        public TargetTypeSelector(Type type, ITargetContainer rootContainer = null)
        {
            _type = type;
            _rootContainer = rootContainer;
        }
    }
}
