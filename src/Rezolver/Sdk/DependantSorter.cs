// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Sdk
{
    internal class DependantSorter<T> : IEnumerable<T>, IComparer<T>
        where T: class
    {
        private readonly IEnumerable<T> Objects;
        private readonly Dictionary<T, HashSet<T>> Map;

        public DependantSorter(IEnumerable<T> objects)
        {
            this.Objects = objects;
            this.Map = new Dictionary<T, HashSet<T>>(ReferenceComparer<T>.Instance);
        }

        HashSet<T> GetDependencyMap(T obj)
        {
            if (!this.Map.TryGetValue(obj, out HashSet<T> toReturn))
            {
                this.Map[obj] = toReturn = this.BuildDependencies(obj);
            }

            return toReturn;
        }

        HashSet<T> BuildDependencies(T obj)
        {
            HashSet<T> entry;
            this.Map[obj] = entry = new HashSet<T>(ReferenceComparer<T>.Instance);
            if (obj is IDependant oDependant)
            {
                // note that the dependencies returned by this function might not be in the
                // Objects enumerable (required objects not present in the input set) - but the
                // algorithm doesn't care
                foreach (var dependency in oDependant.Dependencies.GetDependencies(this.Objects))
                {
                    // allow an object to return itself as a dependency, and ignore it.
                    if (object.ReferenceEquals(obj, dependency))
                    {
                        continue;
                    }

                    entry.Add(dependency);
                    // dependencies of my dependency are my dependencies
                    // (note we don't bother marking transitive dependencies because
                    // they're not relevant to the algorithm)
                    foreach (var dep in this.GetDependencyMap(dependency))
                    {
                        entry.Add(dep);
                    }
                }
            }

            return entry;
        }

        int IComparer<T>.Compare(T x, T y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            // do this before checking reference equality - ensures the dependency maps are built
            // for all objects
            var xdeps = this.GetDependencyMap(x);
            var ydeps = this.GetDependencyMap(y);
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }

            // x is less than y if y depends on x
            // y is less than x if x depends on y
            // if there's no dependency relationship, the one with the fewest dependencies wins
            // an exception occurs if they are codependent
            if (xdeps.Contains(y))
            {
                if (ydeps.Contains(x))
                {
                    throw new DependencyException($"Objects {x} and {y} are mutually dependent - cyclic dependencies are not allowed.");
                }

                return 1;
            }
            else if (ydeps.Contains(x))
            {
                return -1;
            }

            return Comparer<int>.Default.Compare(xdeps.Count, ydeps.Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // force generation of dependency map in the rare case that there was only one thing in the
            // enumerable
            foreach (var result in this.Objects.OrderBy(b => b, this))
            {
                this.GetDependencyMap(result);
                yield return result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
