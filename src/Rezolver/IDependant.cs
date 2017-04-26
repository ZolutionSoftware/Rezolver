using System;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Common interface for an object which has dependencies on one or more other objects of a given type.
    /// 
    /// Used principally for container and target container behaviours.
    /// </summary>
    /// <typeparam name="TDependency"></typeparam>
    public interface IDependant<TDependency>
    {
        /// <summary>
        /// Selects the dependencies for this object which appear in <paramref name="objects"/>.
        /// </summary>
        /// <param name="objects">All objects from which dependencies are to be selected.  The set will
        /// likely include this object too.</param>
        /// <returns>A non-null enumerable of all objects in <paramref name="objects"/> on which this
        /// object depends.  If no dependencies are found, then an empty enumerable is returned.</returns>
        /// <exception cref="InvalidOperationException">If an required dependency is not
        /// found in the <paramref name="objects"/></exception>
        IEnumerable<TDependency> GetDependencies(IEnumerable<TDependency> objects);
    }
}