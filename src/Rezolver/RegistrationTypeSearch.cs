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
    public class RegistrationTypeSearch : IEnumerable<Type>
    {
        private class GenericTypeSearch
        {
            public GenericTypeSearch Parent { get; }
            public Type Type { get; }
            public Type TypeParameter { get; }

            /// <summary>
            /// Used when including base types in the projection - prevents the recursive call
            /// to DeriveGenericTypeSearchList from including the bases more than once.
            /// </summary>
            public bool DisableContravariance { get; }

            public GenericTypeSearch(Type type, Type typeParameter = null, GenericTypeSearch parent = null, bool disableContravariance = false)
            {
                Parent = parent;
                Type = type;
                TypeParameter = typeParameter;
                DisableContravariance = disableContravariance;
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

            if (!TypeHelpers.ContainsGenericParameters(search.Type))
            {
                bool includeObject = false;

                yield return search.Type;

                if (!search.DisableContravariance && search.TypeParameterIsContravariant)
                {
                    //if it's a class then iterate the bases
                    if (!TypeHelpers.IsInterface(search.Type)
                        && !TypeHelpers.IsValueType(search.Type))
                    {
                        includeObject = true;
                        foreach (var baseClass in TypeHelpers.GetAllBases(search.Type)
                            .SelectMany(t => DeriveGenericTypeSearchList(new GenericTypeSearch(t, search.TypeParameter, search, true))))
                        {
                            if(baseClass != typeof(object))
                                yield return baseClass;
                        }
                    }
                    foreach (var i in TypeHelpers.GetInterfaces(search.Type)
                        .SelectMany(t => DeriveGenericTypeSearchList(new GenericTypeSearch(t, search.TypeParameter, search, true))))
                    {
                        yield return i;
                    }
                    //include object last in the list of bases and interfaces
                    if (includeObject)
                        yield return typeof(object);
                }
            }

            if (!TypeHelpers.IsGenericType(search.Type))
                yield break;

            //for every generic type, there is at least two versions - the closed and the open
            //when you consider, also, that a generic parameter might also be a generic, with multiple
            //versions - you can see that things can get icky.  
            var typeArgs = TypeHelpers.GetGenericArguments(search.Type).Zip(TypeHelpers.GetGenericArguments(search.Type.GetGenericTypeDefinition()),
                (arg, param) => new GenericTypeSearch(arg, param, search));
            var typeParamSearchLists = typeArgs.Select(t => DeriveGenericTypeSearchList(t).ToArray()).ToArray();
            var genericType = search.Type.GetGenericTypeDefinition();
            
            // the first result will be equal to search.Type
            foreach (var combination in CartesianProduct(typeParamSearchLists).Skip(1))
            {
                // the first result will typically be the same type as the search type
                yield return genericType.MakeGenericType(combination.ToArray());
            }
            // then return the open generic
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

        public IEnumerator<Type> GetEnumerator()
        {
            return DeriveGenericTypeSearchList(_type).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly Type _type;

        public RegistrationTypeSearch(Type type)
        {
            _type = type;
        }
    }
}
