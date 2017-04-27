using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Represents an individual dependency for an <see cref="IDependant{TDependency}"/>.
    /// </summary>
    /// <typeparam name="TDependency"></typeparam>
    /// <remarks>
    /// Despite the obvious similarity to <see cref="IDependant{TDependency}"/>, this 
    /// type is specifically intended to be used by that type to represent a single dependency
    /// on a particular object or group of objects for one dependant (the dependency <see cref="Owner"/>).
    /// 
    /// The dependencies are not static (even if the dependency is created for a specific target object) - 
    /// you have to <see cref="Resolve(IEnumerable{TDependency})"/> the dependencies from a specific set of
    /// objects.
    /// 
    /// Note that only direct dependencies are returned - transitive dependencies are not.
    /// </remarks>
    public abstract class Dependency<TDependency>
        where TDependency : class
    {
        protected IDependant<TDependency> Owner { get; }
        protected bool Required { get; }
        public Dependency(IDependant<TDependency> owner, bool required)
        {
            Owner = owner;
            Required = required;
        }
        public abstract IEnumerable<TDependency> Resolve(IEnumerable<TDependency> objects);
    }

}
