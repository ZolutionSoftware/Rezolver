using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Represents a dependency on objects of a specific type, which is itself a sub class of 
    /// <typeparamref name="TDependency"/>.
    /// </summary>
    /// <typeparam name="TDependency">The general type of object upon which the owner of this dependency
    /// is dependent upon.  So, an <see cref="IDependant{TDependency}"/> will pass its <typeparamref name="TDependency"/>
    /// argument to this parameter.</typeparam>
    internal class TypeDependency<TDependency> : Dependency<TDependency>
        where TDependency : class
    {
        private Type Type { get; }
        public TypeDependency(Type type, IDependant<TDependency> owner, bool required)
            : base(owner, required)
        {
            Type = type;
        }

        public override IEnumerable<TDependency> Resolve(IEnumerable<TDependency> objects)
        {
            Type oType;
            DependantBase<TDependency> oDependant;
            int count = 0;
            foreach (var o in objects)
            {
                oType = o.GetType();
                if (!TypeHelpers.IsAssignableFrom(Type, oType))
                    continue;
                // if the object is also a DependantBase<T> object
                // then we examine the other's dependency descriptors - if it also has 
                // a TypeDependency for the same type and our owner matches it, then we
                // don't add a dependency on it because we would end up with a circular
                // dependency.
                // Note we don't check whether our owner is in the objects enumerable - 
                // we just assume that it is (because if it's dependencies are being 
                // calculated, then it should be).
                oDependant = o as DependantBase<TDependency>;
                if (oDependant != null)
                {
                    foreach (var descriptor in oDependant.Dependencies.OfType<TypeDependency<TDependency>>())
                    {
                        if (descriptor.Type == Type && TypeHelpers.IsAssignableFrom(Type, Owner.GetType()))
                            continue;
                    }
                }
                count++;
                yield return o;
            }
            if (Required && count == 0)
                throw new InvalidOperationException($"Object { Owner } requires at least one object of type { Type }");
        }
    }
}
