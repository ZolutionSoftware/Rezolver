using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
    /// Represents a behaviour for an <see cref="IContainer"/> which also uses an 
    /// <see cref="ITargetContainer"/> as the source of its registrations.
	/// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerBehaviour"/>, this is specifically used
    /// when a container is created which also uses an <see cref="ITargetContainer"/> as the source of its
    /// service registrations.
    /// 
    /// A behaviour can be passed to any <see cref="ContainerBase"/> derived object on construction (see the
    /// <see cref="ContainerBase.ContainerBase(IContainerBehaviour, ITargetContainer)"/> constructor).  If one
    /// is not passed then a default is used from the <see cref="GlobalBehaviours"/> class which is pertinent to the 
    /// actual container type being created.  Rezolver has global behaviours for the standalone container
    /// types (via the <see cref="GlobalBehaviours.ContainerBehaviour"/> property) and for overriding containers 
    /// (via the <see cref="GlobalBehaviours.OverridingContainerBehaviour"/> property).</remarks>
	public interface IContainerBehaviour
	{
		/// <summary>
        /// Attaches the behaviour to the <paramref name="container"/> and/or its <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the behaviour is to be attached.</param>
        /// <param name="targets">The <see cref="ITargetContainer"/> for the <paramref name="container"/> to
        /// which the behaviour is to be attached.</param>
		void Attach(IContainer container, ITargetContainer targets);
	}
}