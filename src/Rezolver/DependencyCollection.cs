using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// A collection of dependencies (via the <see cref="Dependency{TDependency}"/> class) for 
    /// an <see cref="IDependant{TDependency}"/>
    /// </summary>
    /// <typeparam name="TDependency">The type of object(s) to be selected by the <see cref="Dependency{TDependency}"/>
    /// objects contained within this collection.</typeparam>
    public class DependencyCollection<TDependency> 
        : IEnumerable<Dependency<TDependency>>
        where TDependency: class
    {
        private readonly List<Dependency<TDependency>> _list 
            = new List<Dependency<TDependency>>();

        /// <summary>
        /// Returns all the <see cref="Dependency{TDependency}"/> objects which have been added
        /// to this collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Dependency<TDependency>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a dependency to this collection.
        /// </summary>
        /// <param name="dependency">Required.  The dependency to be added.</param>
        public void Add(Dependency<TDependency> dependency)
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            _list.Add(dependency);
        }

        /// <summary>
        /// Identifies the objects that match the dependencies in this collection.
        /// </summary>
        /// <param name="objects">The objects from which dependencies are to be identified.</param>
        /// <returns>An enumerable containing the objects (selected from <paramref name="objects"/>) which
        /// match the dependencies in this collection.  If there are no dependency matches, the enumerable
        /// will be empty.</returns>
        public IEnumerable<TDependency> Resolve(IEnumerable<TDependency> objects)
        {
            // note that we filter out duplicates by reference, as it's possible for one
            // object to satisfy multiple dependencies
            return _list.SelectMany(d => d.Resolve(objects))
                .Distinct(ReferenceComparer<TDependency>.Instance);
        }
    }
}
