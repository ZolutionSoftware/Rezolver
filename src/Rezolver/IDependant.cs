using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Common interface for an object which has dependencies on one or more other objects of a given type
    /// </summary>
    /// <typeparam name="TDependency"></typeparam>
    public interface IDependant<TDependency>
    {
        /// <summary>
        /// Called to filter the given enumerable of behaviours to include all the behaviours
        /// on which this one depends before it can be configured.
        /// </summary>
        /// <param name="behaviours"></param>
        /// <returns></returns>
        /// <exception cref="BehaviourException">If an expected dependency is not
        /// found in the <paramref name="behaviours"/></exception>
        IEnumerable<TDependency> GetDependencies(IEnumerable<TDependency> behaviours);
    }
}