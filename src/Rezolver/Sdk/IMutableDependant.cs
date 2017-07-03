namespace Rezolver.Sdk
{
    /// <summary>
    /// Interface for an <see cref="IDependant"/> whose dependencies are described by a
    /// <see cref="DependencyMetadataCollection"/> (and which are, therefore, mutable).
    /// </summary>
    public interface IMutableDependant : IDependant
    {
        /// <summary>
        /// A collection through which dependencies can be added and removed, as well as read.
        /// </summary>
        /// <remarks>The <see cref="DependantExtensions"/> extension methods leverage this property to provide a 
        /// clean interface for adding different types of dependencies to dependant objects.</remarks>
        new DependencyMetadataCollection Dependencies { get; }
    }
}