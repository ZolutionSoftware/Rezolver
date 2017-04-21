using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
    /// Encapsulates a configurable behaviour for an <see cref="IContainer"/> which also uses an 
    /// <see cref="ITargetContainer"/> as the source of its registrations.
	/// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerBehaviour"/>, this is specifically used
    /// when a container is created which also uses an <see cref="ITargetContainer"/> as the source of its
    /// service registrations and other behaviours.  Therefore, the type of configuration performed by
    /// an instance of this interface will not be the same as that performed by <see cref="ITargetContainerBehaviour"/>.
    /// 
    /// For example, to configure compilers you would use this interface; to allow automatic enumerable resolving, you
    /// would use <see cref="ITargetContainerBehaviour"/>.
    /// 
    /// As additional background, many of the container and target container classes use registrations within themselves
    /// to configure their behaviour.  Referring back to compilation, for example, the built-in containers will self-resolve
    /// an instance of a compiler when a target needs compiling.</remarks>
	public interface IContainerBehaviour : IDependantBehaviour<IContainerBehaviour>
	{
		/// <summary>
        /// Called to configure the behaviour for the given container via the given targets.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="targets"></param>
		void Configure(IContainer container, ITargetContainer targets);
	}
}