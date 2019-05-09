using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetTypeMetadataTests
    {
        [Fact]
        public void ShouldProduceMetadata()
        {
            // Arrange


            // Act
            var meta = TargetTypeMetadata.For<SimpleGenericClass<TypeArgs.T1, TypeArgs.T2, TypeArgs.T3>>();

            // Assert
            Assert.NotNull(meta);
        }

        [Fact]
        public void ShouldProduceFuncMetadata()
        {
            // Arrange & Act
            TargetTypeMetadata.Prepare(typeof(Func<string>), typeof(string));
            var meta = TargetTypeMetadata.For<Func<object>>();

            // Assert
            Assert.NotNull(meta);
        }
    }

    [DebuggerDisplay("{CachedMetadata}")]
    public class TargetTypeMetadataBase
    {
        // might need to be a Lazy<TargetTypeMetadata>...
        private protected static PerTypeCache<TargetTypeMetadata> CachedMetadata { get; } = new PerTypeCache<TargetTypeMetadata>(CreateMetadata);

        private static TargetTypeMetadata CreateMetadata(Type t)
        {
            if (!t.ContainsGenericParameters)
                return (TargetTypeMetadata)Activator.CreateInstance(typeof(TargetTypeMetadata<>).MakeGenericType(t));
            else
                return (TargetTypeMetadata)Activator.CreateInstance(typeof(OpenGenericMetadata), t);
        }

        private protected TargetTypeMetadataBase() { }
    }

    [DebuggerDisplay("{Type}")]
    public class TargetTypeMetadata : TargetTypeMetadataBase
    {
        public static readonly TargetTypeMetadata[] Empty = new TargetTypeMetadata[0];

        private protected TargetTypeMetadata()
        {
            GenericArguments = new Lazy<TargetTypeMetadata[]>(() => Type.IsGenericType && !Type.ContainsGenericParameters ? Type.GetGenericArguments().Select(ga => CachedMetadata.Get(ga)).ToArray() : Empty);
        }

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
        public TargetTypeMetadata Base { get; protected set; }

        /// <summary>
        /// Only includes *immediate* derived types
        /// </summary>
        public HashSet<TargetTypeMetadata> DerivedTypes { get; protected set; } = new HashSet<TargetTypeMetadata>();

        /// <summary>
        /// Metadata for the generic type definition, if this type is a closed generic.
        /// 
        /// Note - <c>null</c> if this type is a generic type definition, or is not a generic.
        /// </summary>
        public TargetTypeMetadata GenericTypeDefinition { get; protected set; }

        /// <summary>
        /// If the type is a generic type, then this will be the metadata for the arguments.
        /// 
        /// If the type is not a generic type, then this array will be empty.
        /// 
        /// This is lazy because arguments can be recursive - e.g. `class MyType : BaseType&lt;MyType&gt;`
        /// </summary>
        public Lazy<TargetTypeMetadata[]> GenericArguments { get; }

        /// <summary>
        /// Generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TargetTypeMetadata[] GenericInterfaces { get; protected set; }

        /// <summary>
        /// Non-generic interfaces implemented or inferred (if it's an interface) by this type
        /// </summary>
        public TargetTypeMetadata[] NonGenericInterfaces { get; protected set; }

        /// <summary>
        /// Classes or value types which implement this interface
        /// </summary>
        public HashSet<TargetTypeMetadata> ImplementingTypes { get; protected set; } = new HashSet<TargetTypeMetadata>();

        /// <summary>
        /// Other interfaces which infer this interface
        /// </summary>
        public HashSet<TargetTypeMetadata> ImplementingInterfaces { get; protected set; } = new HashSet<TargetTypeMetadata>();

        public override bool Equals(object obj)
        {
            return Type.Equals((obj as TargetTypeMetadata)?.Type);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public static TargetTypeMetadata For<T>()
        {
            return CachedMetadata.Get(typeof(T));
        }

        public static TargetTypeMetadata For(Type type)
        {
            return CachedMetadata.Get(type);
        }

        /// <summary>
        /// Ensures metadata is built for the type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type whose metadata is required.</typeparam>
        public static void Prepare<T>()
        {
            CachedMetadata.Get(typeof(T));
        }

        public static void Prepare(params Type[] types) => Prepare((IEnumerable<Type>)types);

        public static void Prepare(IEnumerable<Type> types)
        {
            foreach(var type in types)
            {
                CachedMetadata.Get(type);
            }
        }
    }

    internal class OpenGenericMetadata : TargetTypeMetadata
    {
        public OpenGenericMetadata(Type type)
        {
            Type = type;
            if (type.IsClass)
            {
                if (type != typeof(object))
                {
                    Base = CachedMetadata.Get(type.BaseType);
                    Base.DerivedTypes.Add(this);
                }
            }

            var interfaces = type.GetInterfaces().Select(iface => CachedMetadata.Get(iface)).ToArray();

            if (Type.IsInterface)
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

            GenericInterfaces = interfaces.Where(iface => iface.Type.IsGenericType).ToArray();
            NonGenericInterfaces = interfaces.Where(iface => !iface.Type.IsGenericType).ToArray();

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                GenericTypeDefinition = CachedMetadata.Get(type.GetGenericTypeDefinition());
            }
        }
    }

    internal class TargetTypeMetadata<T> : TargetTypeMetadata
    {
        private static readonly Type type = typeof(T);
        private static readonly TargetTypeMetadata baseType;
        private static readonly TargetTypeMetadata genericTypeDefinition;
        private static readonly TargetTypeMetadata[] interfaces;

        static TargetTypeMetadata()
        {
            if (type.IsClass)
            {
                if (type != typeof(object))
                {
                    baseType = CachedMetadata.Get(type.BaseType);
                }
            }

            interfaces = type.GetInterfaces().Select(iface => CachedMetadata.Get(iface)).ToArray();

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                genericTypeDefinition = CachedMetadata.Get(type.GetGenericTypeDefinition());
            }
        }

        public TargetTypeMetadata()
        {
            Type = typeof(T);

            if (baseType != null)
            {
                baseType.DerivedTypes.Add(this);
                Base = baseType;
            }

            GenericTypeDefinition = genericTypeDefinition;

            if (Type.IsInterface)
            {
                foreach (var iface in interfaces)
                {
                    iface.ImplementingTypes.Add(this);
                }
            }
            else
            {
                foreach(var iface in interfaces)
                {
                    iface.ImplementingInterfaces.Add(this);
                }
            }

            GenericInterfaces = interfaces.Where(iface => iface.Type.IsGenericType).ToArray();
            NonGenericInterfaces = interfaces.Where(iface => !iface.Type.IsGenericType).ToArray();
        }
    }
}
