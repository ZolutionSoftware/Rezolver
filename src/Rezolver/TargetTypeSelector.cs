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
            //IFoo<IEnumerable<IBar<T[Base0..n]>>>,    //foreach base of T if contravariant
            //IFoo<IEnumerable<IBar<>>,
            //IFoo<IEnumerable<>
            //IFoo<>

            //using an iterator method is not the best for performance, but fetching type
            //registrations from a container builder is an operation that, so long as a caching
            //resolver is used, shouldn't be repeated often - plus for single
            yield return search.Type;

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

                // TODO: process generic constraints
                // the first result will be equal to search.Type
                foreach (var combination in Permutate(typeParamSearchLists).Skip(1))
                {
                    // the first result will typically be the same type as the search type
                    yield return genericType.MakeGenericType(combination.ToArray());
                }

                yield return genericType;
            }

            if (search.EnableVariance
                && search.TypeParameterIsContravariant
                && (search.Contravariance & Contravariance.BasesAndInterfaces) != Contravariance.None)
            {
                // note - when spawning child searches, this search's parent is passed as the parent
                // to the newly created search.  This ensures that the contravariant search direction
                // is calculated correctly

                bool appendObject = false;

                if ((search.Contravariance & Contravariance.Bases) == Contravariance.Bases)
                {
                    //if it's a class then iterate the bases
                    if (!TypeHelpers.IsInterface(search.Type)
                        && !TypeHelpers.IsValueType(search.Type)
                        && search.Type != typeof(object))
                    {
                        appendObject = true;
                        // note - disable interfaces when recursing into the base
                        // note also - ignore 'object'
                        foreach (var t in 
                            Run(new SearchParams(TypeHelpers.BaseType(search.Type), search.TypeParameter, search.Parent, Contravariance.Bases))
                            .Where(tt => tt != typeof(object)))
                        {
                            yield return t;
                        }
                    }
                }

                if ((search.Contravariance & Contravariance.Interfaces) == Contravariance.Interfaces)
                {
                    foreach (var i in TypeHelpers.GetInterfaces(search.Type)
                        .SelectMany(t => Run(new SearchParams(t, search.TypeParameter, search.Parent, Contravariance.Bases))))
                    {
                        yield return i;
                    }
                }
                //include object last in the list of bases and interfaces
                if (appendObject)
                    yield return typeof(object);
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
            return Run(_type).GetEnumerator();
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
