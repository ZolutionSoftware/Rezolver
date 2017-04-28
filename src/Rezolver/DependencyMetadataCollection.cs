using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// A collection of <see cref="DependencyMetadata"/> objects.
    /// 
    /// An <see cref="IDependant"/> contains one of these collections in order to store the
    /// dependencies that it has on other objects.
    /// </summary>
    public class DependencyMetadataCollection : IEnumerable<DependencyMetadata>
    {
        private readonly List<DependencyMetadata> _list 
            = new List<DependencyMetadata>();

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/> - returns an enumerator 
        /// which enumerates all the <see cref="DependencyMetadata"/> objects in the collection.
        /// </summary>
        public IEnumerator<DependencyMetadata> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Implementation <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a dependency to this collection.
        /// </summary>
        /// <param name="dependency">Required.  The dependency to be added.</param>
        public void Add(DependencyMetadata dependency)
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            _list.Add(dependency);
        }

        /// <summary>
        /// Adds multiple dependencies to the collection.
        /// </summary>
        /// <param name="dependencies"></param>
        public void AddRange(IEnumerable<DependencyMetadata> dependencies)
        {
            if (dependencies == null) throw new ArgumentNullException(nameof(dependencies));
            //use a temp list to avoid enumerating twice
            List<DependencyMetadata> toAdd = new List<DependencyMetadata>();
            foreach(var dep in dependencies)
            {
                if (dep == null) throw new ArgumentException("All items in the enumerable must be non-null", nameof(dependencies));
                toAdd.Add(dep);
            }
            _list.AddRange(toAdd);
        }

        /// <summary>
        /// Identifies the objects that match the dependencies in this collection.
        /// </summary>
        /// <param name="objects">The objects from which dependencies are to be identified.</param>
        /// <returns>An enumerable containing the objects (selected from <paramref name="objects"/>) which
        /// match the dependencies in this collection.  If there are no dependency matches, the enumerable
        /// will be empty.</returns>
        public IEnumerable<T> GetDependencies<T>(IEnumerable<T> objects)
            where T:class
        {
            // note that we filter out duplicates by reference, as it's possible for one
            // object to satisfy multiple dependencies
            return _list.SelectMany(d => d.GetDependencies(objects))
                .Distinct(ReferenceComparer<T>.Instance);
        }
    }
}
