using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Represents a dependency from an <see cref="IDependant"/> on one or more other objects
    /// in a collection.
    /// 
    /// Note that the current implementations of this class are internal and can only be created
    /// through the various methods in the <see cref="DependantExtensions"/> class.
    /// </summary>
    public abstract class DependencyMetadata
    {
        /// <summary>
        /// The <see cref="IDependant"/> to which this dependency belongs.
        /// </summary>
        protected IDependant Owner { get; }
        /// <summary>
        /// <c>true</c> if the dependency is essential to the <see cref="Owner"/>, false
        /// if it's optional.
        /// </summary>
        /// <remarks>When a <see cref="DependantCollection{T}"/> sorts its contents by dependency,
        /// all objects will be sorted after their dependencies, regardless of whether they're required or
        /// not.  The difference between a required dependency and an optional one is that if a required
        /// dependency fails to match all the objects it expects when <see cref="GetDependencies{T}(IEnumerable{T})"/>
        /// is called, an exception will occur.</remarks>
        protected bool Required { get; }
        /// <summary>
        /// Creates a new instance of the <see cref="DependencyMetadata"/> class.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="required"></param>
        public DependencyMetadata(IDependant owner, bool required)
        {
            Owner = owner;
            Required = required;
        }
        /// <summary>
        /// Called to select the dependencies which match this dependency metadata from the 
        /// <paramref name="objects"/> passed.
        /// </summary>
        /// <typeparam name="T">Type type of objects from which dependencies are sought.</typeparam>
        /// <param name="objects">Required.  The objects which are to be searched for dependencies which
        /// match this dependency metadata.</param>
        /// <returns>A filtered enumerable containing any objects from <paramref name="objects"/> which match
        /// this dependency metadata.</returns>
        public abstract IEnumerable<T> GetDependencies<T>(IEnumerable<T> objects) where T: class;
    }

}
