using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Provides configuration for <see cref="ITargetContainer"/> instances.
    /// </summary>
    /// <remarks>This abstraction provides a way to apply common setup procedures to <see cref="ITargetContainer"/>
    /// objects which are then used by containers and during compilation - typically by registering targets or 
    /// target containers within it.
    /// 
    /// While similar to <see cref="IContainerBehaviour"/>, this is specifically intended to be used when
    /// creating any <see cref="ITargetContainer"/>; whereas the other interface is for when a container is created
    /// which also uses <see cref="ITargetContainer"/>.</remarks>
    public interface ITargetContainerBehaviour
    {
        /// <summary>
        /// Called to configure the given <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to be configured - will not be null when called by the framework.</param>
        void Configure(ITargetContainer targets);
    }
}
