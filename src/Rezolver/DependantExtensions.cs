using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    public static class DependantExtensions
    {
        /// <summary>
        /// Configures this object with a required dependency on the given <paramref name="obj"/>
        /// </summary>
        /// <param name="obj">Required.  The object that is required by this object.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public static IDependant<TDependency> Requires<TDependency>(this IDependant<TDependency> dependant, TDependency obj)
            where TDependency : class, IDependant<TDependency>
        {
            if (dependant == null) throw new ArgumentNullException(nameof(dependant));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            dependant.Dependencies.Add(new ObjectDependency<TDependency>(obj, dependant, true));
            return dependant;
        }

        /// <summary>
        /// Configures this object with a set of required dependencies on the given <paramref name="objects"/>
        /// </summary>
        /// <param name="objects">Required.  The objects that are required by this object.  None of the items can be null.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public static IDependant<TDependency> RequiresAll<TDependency>(this IDependant<TDependency> dependant, IEnumerable<TDependency> objects)
            where TDependency : class
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));
            if (objects.Any(o => o == null)) throw new ArgumentException("All objects must be non-null", nameof(objects));

            foreach (var obj in objects)
            {
                dependant.Dependencies.Add(new ObjectDependency<TDependency>(obj, dependant, true));
            }
            return dependant;
        }

        /// <summary>
        /// Configures this object with a set of required dependencies on the given <paramref name="objects"/>
        /// </summary>
        /// <param name="objects">Required.  The objects that are required by this object.  None of the items can be null.</param>
        /// <returns>The object on which the method is called, to allow further dependencies to be specified fluently.</returns>
        public static IDependant<TDependency> RequiresAll<TDependency>(this IDependant<TDependency> dependant, params TDependency[] objects)
            where TDependency : class
        {
            return RequiresAll(dependant, (IEnumerable<TDependency>)objects);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tOther"></param>
        /// <returns></returns>
        public static IDependant<TDependency> RequiresAny<TDependency>(this IDependant<TDependency> dependant, Type tOther)
            where TDependency : class
        {
            if (tOther == null) throw new ArgumentNullException(nameof(tOther));
            dependant.Dependencies.Add(new TypeDependency<TDependency>(tOther, dependant, true));
            return dependant;
        }
    }
}
