using System;
using System.Collections.Generic;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Interface for an object that is dependent upon other objects
    /// </summary>
    public interface IDependant
    {
        /// <summary>
        /// Contains all dependency metadata for this object.
        /// </summary>
        IEnumerable<DependencyMetadata> Dependencies { get; }
    }
}