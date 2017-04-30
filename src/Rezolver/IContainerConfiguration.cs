using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
    /// Encapsulates a configurable behaviour for an <see cref="IContainer"/> which also uses an 
    /// <see cref="ITargetContainer"/> as the source of its registrations.
	/// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerConfiguration"/>, this is specifically used
    /// when a container is created which also uses an <see cref="ITargetContainer"/> as the source of its
    /// service registrations.  Therefore, the type of configuration performed by
    /// an instance of this interface will not be the same as that performed by <see cref="ITargetContainerConfiguration"/>.
    /// 
    /// A configuration object can be passed to any <see cref="ContainerBase"/> derived object on construction (see the
    /// <see cref="ContainerBase.ContainerBase(ITargetContainer, IContainerConfiguration)"/> constructor).  If one
    /// is not passed then a default is used from the <see cref="DefaultConfiguration"/> class which is pertinent to the 
    /// actual container type being created.  Rezolver has default configuration collections for the standalone container
    /// types (via the 
    /// <see cref="DefaultConfiguration.ContainerConfig"/> property) and for overriding containers 
    /// (via the <see cref="DefaultConfiguration.OverridingContainerConfig"/> property).
    /// standalone containers a</remarks>
	public interface IContainerConfiguration
	{
		/// <summary>
        /// Called to configure the behaviour for the given container via the given targets.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="targets"></param>
		void Configure(IContainer container, ITargetContainer targets);
	}
}