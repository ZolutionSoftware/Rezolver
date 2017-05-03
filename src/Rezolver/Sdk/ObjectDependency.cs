using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Represents a dependency on a specific instance
    /// </summary>
    internal class ObjectDependency : DependencyMetadata
    {
        private object Obj { get; }

        public ObjectDependency(object obj, IDependant owner, bool required)
            : base(owner, required)
        {
            Obj = obj;
        }

        public override IEnumerable<T> GetDependencies<T>(IEnumerable<T> objects)
        {
            bool found = false;
            foreach (var result in objects.Where(o => object.ReferenceEquals(Obj, o)))
            {
                found = true;
                yield return result;
            }

            if (Required && !found)
                throw new DependencyException($"Object { Owner } has a required dependency on { Obj } which was not found");
        }
    }
}
