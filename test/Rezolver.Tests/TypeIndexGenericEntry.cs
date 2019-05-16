using System;
using System.Linq;

namespace Rezolver.Tests
{
    internal class TypeIndexGenericEntry : TypeIndexEntry
    {
        public TypeIndexGenericEntry(TypeIndex index, Type type)
            : base(index)
        {
            Type = type;
            if (type.IsClass)
            {
                if (type != typeof(object))
                {
                    Base = index.For(type.BaseType);
                    Base.DerivedTypes.Add(this);
                }
            }

            var interfaces = type.GetInterfaces().Select(iface => index.For(iface)).ToArray();

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
                GenericTypeDefinition = index.For(type.GetGenericTypeDefinition());
            }
        }
    }
}
