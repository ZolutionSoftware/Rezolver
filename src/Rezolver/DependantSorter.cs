using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    internal class DependantSorter<T> : IEnumerable<T>, IComparer<T>
        where T : class, IDependant<T>
    {
        private readonly IEnumerable<T> Objects;
        private readonly Dictionary<T, HashSet<T>> Map;

        public DependantSorter(IEnumerable<T> objects)
        {
            Objects = objects;
            Map = new Dictionary<T, HashSet<T>>(ReferenceComparer<T>.Instance);
        }

        HashSet<T> GetDependencyMap(T obj)
        {
            HashSet<T> toReturn;
            if (!Map.TryGetValue(obj, out toReturn))
                Map[obj] = toReturn = BuildDependencies(obj);
            return toReturn;
        }

        HashSet<T> BuildDependencies(T obj)
        {
            HashSet<T> entry;
            Map[obj] = entry = new HashSet<T>(ReferenceComparer<T>.Instance);
            foreach (var dependency in obj.Dependencies.Resolve(Objects))
            {
                // allow an object to return itself as a dependency, and ignore it.
                if (object.ReferenceEquals(obj, dependency))
                    continue;
                entry.Add(dependency);
                // dependencies of my dependency are my dependencies :)
                foreach (var dep in GetDependencyMap(dependency))
                {
                    entry.Add(dep);
                }
            }
            return entry;
        }

        int IComparer<T>.Compare(T x, T y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            // do this before checking reference equality - ensures the dependency maps are built
            // for all objects
            var xdeps = GetDependencyMap(x);
            var ydeps = GetDependencyMap(y);
            if (object.ReferenceEquals(x, y))
                return 0;
            // x is less than y if y depends on x
            // y is less than x if x depends on y
            // if there's no dependency relationship, the one with the fewest dependencies wins
            // an exception occurs if they are codependant
            if (xdeps.Contains(y))
            {
                if (ydeps.Contains(x))
                    throw new InvalidOperationException($"Objects {x} and {y} are mutually dependent - cyclic dependencies are not allowed.");
                return 1;
            }
            else if (ydeps.Contains(x))
                return -1;
            return Comparer<int>.Default.Compare(xdeps.Count, ydeps.Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Objects.OrderBy(b => b, this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
