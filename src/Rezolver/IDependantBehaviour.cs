using System.Collections.Generic;

namespace Rezolver
{
    public interface IDependantBehaviour<T> where T: IDependantBehaviour<T>
    {
        /// <summary>
        /// Called to filter the given enumerable of behaviours to include all the behaviours
        /// on which this one depends before it can be configured.
        /// </summary>
        /// <param name="behaviours"></param>
        /// <returns></returns>
        /// <exception cref="BehaviourConfigurationException">If an expected dependency is not
        /// found in the <paramref name="behaviours"/></exception>
        IEnumerable<T> GetDependencies(IEnumerable<T> behaviours);
    }
}