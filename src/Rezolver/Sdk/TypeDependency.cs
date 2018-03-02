// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Sdk
{
    internal class TypeDependency : DependencyMetadata
    {
        private Type Type { get; }

        public TypeDependency(Type type, IDependant owner, bool required)
            : base(owner, required)
        {
            this.Type = type;
        }

        public override IEnumerable<T> GetDependencies<T>(IEnumerable<T> objects)
        {
            int count = 0;
            List<T> allSkipped = new List<T>();
            bool match;
            foreach (var o in objects)
            {
                match = !object.ReferenceEquals(this.Owner, o) && TypeHelpers.IsAssignableFrom(this.Type, o.GetType());

                if (match && o is IDependant oDependant)
                {
                    // if the object is also an IDependant object
                    // then we examine its dependency descriptors - if it also has
                    // a TypeDependency for the same type and our owner matches it, then we
                    // don't add a dependency on it because we would end up with a circular
                    // dependency.
                    // Note we don't check whether our owner is in the objects enumerable -
                    // we just assume that it is (because if its dependencies are being
                    // calculated, then it should be).
                    foreach (var oDependency in oDependant.Dependencies.OfType<TypeDependency>())
                    {
                        if (oDependency.Type == this.Type && TypeHelpers.IsAssignableFrom(this.Type, this.Owner.GetType()))
                        {
                            match = false;

                            // mark that we skipped this object - helps format a more context-sensitive exception
                            // later if we need to
                            allSkipped.Add(o);

                            break;
                        }
                    }
                }

                if (match)
                {
                    count++;
                    yield return o;
                }
            }

            if (this.Required && count == 0)
            {
                string msg = $"{this.Owner} requires at least one object of type {this.Type}";
                if (allSkipped.Count != 0)
                {
                    msg = $"{msg} - {allSkipped.Count} matching object(s) ignored because they also have an identical type dependency which matches the owner.";
                }

                throw new DependencyException(msg);
            }
        }
    }
}
