using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rezolver.Tests
{
    public enum VarianceSearch
    {
        BaseTypesAndInterfaces = -1,
        Initial = 0,
        DerivedTypesAndImplementations = 1,
        Disabled = 2
    }

    [DebuggerDisplay("{Type}")]
    public class TypeIndexEntry
    {
        public static readonly (Type, TypeIndexEntry)[] EmptyArgs = new (Type, TypeIndexEntry)[0];

        /// <summary>
        /// The type that this metadata describes
        /// </summary>
        public Type Type { get; protected set; }

        /// <summary>
        /// If the type is a class, this is the metadata for its base class.
        /// 
        /// Note - Value Types and Interfaces will have <c>null</c>.
        /// All other types terminate at the metadata for <see cref="object"/>, which will also have a <c>null</c>
        /// </summary>
        public TypeIndexEntry Base { get; protected set; }

        /// <summary>
        /// Gets all the bases of this type in order, starting with <see cref="Base"/>
        /// </summary>
        public IEnumerable<TypeIndexEntry> Bases
        {
            get
            {
                var next = Base;
                while (next != null)
                {
                    yield return next;
                    next = next.Base;
                }
            }
        }

        /// <summary>
        /// Only includes *immediate* derived types
        /// </summary>
        public List<TypeIndexEntry> DerivedTypes { get; protected set; } = new List<TypeIndexEntry>();

        /// <summary>
        /// Gets an enumerable of all the types which are derived from this type, in *generally* increasing order of inheritance.
        /// 
        /// So, the <see cref="DerivedTypes"/> come first, and then each of their derived types, and so on.
        /// 
        /// However, derived generic types are not ordered by inheritance of their generic arguments.
        /// </summary>
        public IEnumerable<TypeIndexEntry> AllDerivedTypes
        {
            get
            {
                // breadth-first enumerable of derived types
                List<TypeIndexEntry> nextLevel = new List<TypeIndexEntry>() { this };

                while (nextLevel.Count != 0)
                {
                    List<TypeIndexEntry> temp = new List<TypeIndexEntry>();
                    foreach (var next in nextLevel)
                    {
                        foreach (var derived in next.DerivedTypes)
                        {
                            yield return derived;
                            temp.Add(derived);
                        }
                    }
                    nextLevel = temp;
                }
            }
        }

        /// <summary>
        /// Metadata for the generic type definition, if this type is a closed generic.
        /// 
        /// Note - <c>null</c> if this type is a generic type definition, or is not a generic.
        /// </summary>
        public TypeIndexEntry GenericTypeDefinition { get; protected set; }

        public HashSet<TypeIndexEntry> KnownGenerics { get; } = new HashSet<TypeIndexEntry>();

        /// <summary>
        /// If the type is a generic type, then this will be the metadata for the arguments.
        /// 
        /// If the type is not a generic type, then this array will be empty.
        /// 
        /// This is lazy because arguments can be recursive - e.g. `class MyType : BaseType&lt;MyType&gt;`
        /// </summary>
        public Lazy<(Type param, TypeIndexEntry argMeta)[]> GenericParametersAndArgs { get; }

        /// <summary>
        /// Generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TypeIndexEntry[] GenericInterfaces { get; protected set; }

        /// <summary>
        /// Non-generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TypeIndexEntry[] NonGenericInterfaces { get; protected set; }

        /// <summary>
        /// Classes or value types which implement this interface
        /// </summary>
        public HashSet<TypeIndexEntry> ImplementingTypes { get; } = new HashSet<TypeIndexEntry>();

        /// <summary>
        /// Other interfaces which infer this interface
        /// </summary>
        public HashSet<TypeIndexEntry> ImplementingInterfaces { get; } = new HashSet<TypeIndexEntry>();
        public TypeIndex Index { get; }

        private protected TypeIndexEntry(TypeIndex index)
        {
            GenericParametersAndArgs = new Lazy<(Type, TypeIndexEntry)[]>(
                () => Type.IsGenericType && !Type.IsGenericTypeDefinition ? Type.GetGenericArguments().Select(ga => (ga, Index.For(ga))).ToArray() : EmptyArgs);
            Index = index;
        }

        public override bool Equals(object obj)
        {
            return Type.Equals((obj as TypeIndexEntry)?.Type);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public VarianceSearch SelectVariance(Type typeParameter, VarianceSearch current)
        {
            if (current == VarianceSearch.Disabled || !typeParameter.IsVariantTypeParameter())
                return VarianceSearch.Disabled;

            throw new NotSupportedException("In development");

            //if (typeParameter.IsCovariantTypeParameter())
            //    return current == VarianceSearch.Initial ? VarianceSearch.DerivedTypesAndImplementations : current;
            //else if (typeParameter.IsContravariantTypeParameter())


        }

        public IEnumerable<TypeIndexEntry> GetTargetSearchTypeMetadatas(Type typeParameter = null, VarianceSearch variance = VarianceSearch.Initial)
        {
            if (GenericTypeDefinition != null)
            {
                //var argsSearches = GenericParametersAndArgs.Value.Select(pa => pa.argMeta.GetTargetSearchTypeMetadatas(pa.param).ToArray()).ToArray();
                //var permutated = argsSearches.Permutate().ToArray();

                // recurse for the type arguments, permutating the results.
                foreach (var genericMeta in GenericParametersAndArgs.Value.Select(pa => pa.argMeta.GetTargetSearchTypeMetadatas(pa.param))
                    .Permutate()
                    .Select(typeArgs => Index.For(GenericTypeDefinition.Type.MakeGenericType(typeArgs.Select(a => a.Type).ToArray()))))
                {
                    yield return genericMeta;
                }

                yield return GenericTypeDefinition;
            }
            else
            {
                yield return this;
            }

            if (variance != VarianceSearch.Disabled)
            {
                if (typeParameter != null)
                {
                    //variance
                }
            }

            if (typeParameter?.IsVariantTypeParameter() ?? false && variance != VarianceSearch.Disabled)
            {

            }
        }

        public IEnumerable<Type> GetTargetSearchTypes(Type typeParameter = null)
        {
            return this.GetTargetSearchTypeMetadatas(typeParameter).Select(m => m.Type);
        }
    }
}
