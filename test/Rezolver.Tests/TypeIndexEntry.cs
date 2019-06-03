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
    public sealed partial class TypeIndexEntry : IEquatable<TypeIndexEntry>, IEquatable<Type>
    {
        public static readonly (Type, TypeIndexEntry)[] EmptyArgs = new (Type, TypeIndexEntry)[0];

        /// <summary>
        /// The type that this metadata describes
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// If the type is a class, this is the metadata for its base class.
        /// 
        /// Note - Value Types and Interfaces will have <c>null</c>.
        /// All other types terminate at the metadata for <see cref="object"/>, which will also have a <c>null</c>
        /// </summary>
        public TypeIndexEntry Base { get; set; }

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
        public HashSet<TypeIndexEntry> DerivedTypes { get; } = new HashSet<TypeIndexEntry>();

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
                var nextLevel = new List<TypeIndexEntry>() { this };

                while (nextLevel.Count != 0)
                {
                    var temp = new List<TypeIndexEntry>();
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

        public bool IsValueType => Type.IsValueType;

        public bool IsClass => Type.IsClass;

        public bool IsInterface => Type.IsInterface;

        /// <summary>
        /// Metadata for the generic type definition, if this type is a closed generic.
        /// 
        /// Note - <c>null</c> if this type is a generic type definition, or is not a generic.
        /// </summary>
        public TypeIndexEntry GenericTypeDefinition { get; set; }

        public HashSet<TypeIndexEntry> KnownGenerics { get; } = new HashSet<TypeIndexEntry>();

        /// <summary>
        /// If the type is a generic type, then this will be the metadata for the arguments.
        /// 
        /// If the type is not a generic type, then this array will be empty.
        /// 
        /// This is lazy because arguments can be recursive - e.g. `class MyType : BaseType&lt;MyType&gt;`
        /// </summary>
        //public Lazy<(Type param, TypeIndexEntry argMeta)[]> GenericParametersAndArgs { get; }
        public (Type param, TypeIndexEntry argMeta)[] GenericParametersAndArgs { get; private set; }

        /// <summary>
        /// Generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TypeIndexEntry[] GenericInterfaces { get; set; }

        /// <summary>
        /// Non-generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TypeIndexEntry[] NonGenericInterfaces { get; set; }

        /// <summary>
        /// Classes or value types which implement this interface
        /// </summary>
        public HashSet<TypeIndexEntry> ImplementingTypes { get; } = new HashSet<TypeIndexEntry>();

        /// <summary>
        /// Other interfaces which infer this interface
        /// </summary>
        public HashSet<TypeIndexEntry> ImplementingInterfaces { get; } = new HashSet<TypeIndexEntry>();

        public TypeIndex Index { get; set; }

        internal TypeIndexEntry(TypeIndex index, Type type)
        {
            Index = index;
            Type = type;

            //GenericParametersAndArgs = new Lazy<(Type, TypeIndexEntry)[]>(
            //    () => Type.IsGenericType && !Type.IsGenericTypeDefinition ? Type.GetGenericArguments().Select(ga => (ga, Index.For(ga))).ToArray() : EmptyArgs);
        }

        internal void Initialise(Dictionary<Type, TypeIndexEntry> beingBuilt)
        {
            beingBuilt[Type] = this;

            if (Type.IsClass)
            {
                if (Type != typeof(object))
                {
                    Base = Index.For(Type.BaseType);
                    Base.DerivedTypes.Add(this);
                }
            }

            var interfaces = Type.GetInterfaces().Select(iface => Index.For(iface, beingBuilt)).ToArray();

            GenericInterfaces = interfaces.Where(iface => iface.Type.IsGenericType).ToArray();
            NonGenericInterfaces = interfaces.Where(iface => !iface.Type.IsGenericType).ToArray();

            if (!Type.IsInterface)
            {
                foreach (var iface in interfaces)
                {
                    iface.ImplementingTypes.Add(this);
                }
            }
            else
            {
                foreach (var iface in interfaces)
                {
                    iface.ImplementingInterfaces.Add(this);
                }
            }

            if (Type.IsGenericType && !Type.IsGenericTypeDefinition)
            {
                GenericTypeDefinition = Index.For(Type.GetGenericTypeDefinition(), beingBuilt);
                GenericTypeDefinition.KnownGenerics.Add(this);
            }



            GenericParametersAndArgs =
                Type.IsGenericType && !Type.IsGenericTypeDefinition ?
                Type.GetGenericTypeDefinition()
                    .GetGenericArguments()
                    .Zip(Type.GetGenericArguments(), (p, a) => (param: p, arg: a))
                    .Select(pa => (pa.param, Index.For(pa.arg, beingBuilt))).ToArray() : EmptyArgs;
        }

        public bool Equals(TypeIndexEntry other)
        {
            return Type.Equals(other?.Type);
        }

        public bool Equals(Type other)
        {
            return Type.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is TypeIndexEntry entry ? Equals(entry) : Equals(obj as Type);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public IEnumerable<(Type type, bool isVariant)> GetCompatibleTypes()
        {
            return GetCompatibleEntries().Select(ev => (ev.entry.Type, ev.isVariant));
        }

        public IEnumerable<(TypeIndexEntry entry, bool isVariant)> GetCompatibleEntries()
        {
            var search = new TypeIndexSearch(typeParameter: null);
            GetCompatibleEntries(search);
            return search.Results.Distinct();
        }

        private void GetCompatibleEntries(in TypeIndexSearch search)
        {
            if (GenericTypeDefinition != null)
            {
                var searchCopy = search;
                // recurse for the type arguments, permutating the results.
                foreach (var (type, isVariant) in GenericParametersAndArgs
                    .Select(pa =>
                    {
                        var argSearch = new TypeIndexSearch(
                            parent: in searchCopy,
                            includeClassVariants: true,
                            includeInterfaceVariants: true,
                            includeGenericDefinition: true,
                            typeParameter: pa.param,
                            useNewResultsList: true);

                        pa.argMeta.GetCompatibleEntries(argSearch);

                        return argSearch.Results;
                    })
                    .Permutate()
                    .Select(typeArgs => (
                        type: GenericTypeDefinition.Type.MakeGenericType(typeArgs.Select(a => a.entry.Type).ToArray()), 
                        isVariant: typeArgs.Any(ev => ev.variantMatch)))
                    .Where(result => !searchCopy.KnownGenericsOnly ||
                                    //(searchCopy.IncludeGenericDefinition && result.type.ContainsGenericParameters) ||
                                    Index.Has(result.type))
                    .Select(result => (Index.For(result.type), result.isVariant)))
                {
                    if (type.Equals(this))
                        search.AddResult(type, false);
                    else
                        search.AddResult(type, isVariant);
                }

                if (search.IncludeGenericDefinition && GenericTypeDefinition != null)
                {
                    search.AddResult(GenericTypeDefinition);
                }
            }
            else
            {
                search.AddResult(this);
            }

            // gets only concrete types to which this type can be assigned by reference.
            // therefore value types are excluded.
            if (Type.IsValueType)
                return;

            TypeIndexEntry objectEntry = null;
            if (search.IncludeClassVariants)
            {
                if (search.VarianceDirection < 0)
                {
                    if (Base != null && Base.Equals(typeof(object)))
                        objectEntry = Base;
                    else
                        search.AddResult(Base, true);

                    Base.GetCompatibleEntries(new TypeIndexSearch(
                        parent: in search,
                        includeClassVariants: true,
                        includeInterfaceVariants: false,
                        includeGenericDefinition: search.IncludeGenericDefinition,
                        typeParameter: search.TypeParameter,
                        resultsAreVariant: true));
                }
                else if (search.VarianceDirection > 0)
                {
                    if (IsClass)
                        search.AddResults(AllDerivedTypes, true);
                    else if (IsInterface)
                        search.AddResults(ImplementingTypes.Where(t => !t.IsValueType), true);
                }
            }

            if (search.IncludeInterfaceVariants)
            {
                if (search.VarianceDirection < 0)
                {
                    search.AddResults(GenericInterfaces, true);
                    search.AddResults(NonGenericInterfaces, true);
                }
                else if (search.VarianceDirection > 0)
                {
                    search.AddResults(ImplementingInterfaces, true);
                }
            }

            if (objectEntry != null)
                search.AddResult(objectEntry, true);
        }
    }
}
