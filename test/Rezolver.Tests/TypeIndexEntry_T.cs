using System;
using System.Linq;

namespace Rezolver.Tests
{
    internal class TypeIndexEntry<T> : TypeIndexEntry
    {
        public TypeIndexEntry(TypeIndex index)
            : base(index)
        {
            Type = typeof(T);

            if (Type.IsClass)
            {
                if (Type != typeof(object))
                {
                    Base = Index.For(Type.BaseType);
                    Base.DerivedTypes.Add(this);
                }
            }

            var interfaces = Type.GetInterfaces().Select(iface => index.For(iface)).ToArray();

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
                GenericTypeDefinition = index.For(Type.GetGenericTypeDefinition());
                GenericTypeDefinition.KnownGenerics.Add(this);
            }
        }
    }
}
