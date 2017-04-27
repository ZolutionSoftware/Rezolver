using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Represents a dependency on a specific instance
    /// </summary>
    internal class ObjectDependency<TDependency> : Dependency<TDependency>
        where TDependency : class
    {
        private TDependency Obj { get; }

        public ObjectDependency(TDependency obj, IDependant<TDependency> owner, bool required)
            : base(owner, required)
        {
            Obj = obj;
        }

        public override IEnumerable<TDependency> Resolve(IEnumerable<TDependency> objects)
        {
            bool found = false;
            foreach (var result in objects.Where(o => object.ReferenceEquals(Obj, o)))
            {
                found = true;
                yield return result;
            }

            if (Required && !found)
                throw new InvalidOperationException($"Object { Owner } has a required dependency on { Obj } which was not found");
        }
    }
}
