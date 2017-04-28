using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    internal class TypeDependency : DependencyMetadata
    {
        private Type Type { get; }
        public TypeDependency(Type type, IDependant owner, bool required)
            : base(owner, required)
        {
            Type = type;
        }

        public override IEnumerable<T> GetDependencies<T>(IEnumerable<T> objects)
        {
            int count = 0;
            foreach (var o in objects)
            {
                if (!TypeHelpers.IsAssignableFrom(Type, o.GetType()))
                    continue;
                // if the object is also an IDependant object
                // then we examine its dependency descriptors - if it also has 
                // a TypeDependency for the same type and our owner matches it, then we
                // don't add a dependency on it because we would end up with a circular
                // dependency.
                // Note we don't check whether our owner is in the objects enumerable - 
                // we just assume that it is (because if it's dependencies are being 
                // calculated, then it should be).
                if (o is IDependant oDependant)
                {
                    foreach (var oDependency in oDependant.Dependencies.OfType<TypeDependency>())
                    {
                        if (oDependency.Type == Type && TypeHelpers.IsAssignableFrom(Type, Owner.GetType()))
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
