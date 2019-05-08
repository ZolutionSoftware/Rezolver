// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq;
using System.Collections.Generic;

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
            {
                throw new DependencyException($"{Owner} has a required dependency on {Obj} which was not found");
            }
        }
    }
}
