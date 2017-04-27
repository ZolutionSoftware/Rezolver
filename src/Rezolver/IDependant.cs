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
        where TDependency : class
    {
        /// <summary>
        /// The dependencies for this object.  To obtain the collection of <typeparamref name="TDependency" />
        /// on which this object depends - you must call <see cref="DependencyCollection{TDependency}.Resolve(IEnumerable{TDependency})"/>,
        /// passing the collection of objects from which dependencies are to be selected.
        /// </summary>
        DependencyCollection<TDependency> Dependencies { get; }
    }
}