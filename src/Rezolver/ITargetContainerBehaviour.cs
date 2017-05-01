using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Represents a behaviour that can be attached to <see cref="ITargetContainer"/> instances to
    /// customise how it behaves.
    /// </summary>
    /// <remarks>This abstraction provides a way to customise and extend an <see cref="ITargetContainer"/>;
    /// typically by registering one or more <see cref="ITarget"/> or <see cref="ITargetContainer"/>
    /// objects in the target container passed to <see cref="Attach(ITargetContainer)"/>.
    /// 
    /// While similar to <see cref="IContainerBehaviour"/>, this is specifically intended to be used when
    /// creating any <see cref="ITargetContainer"/>; whereas the other interface is for when a container is created
    /// which also uses <see cref="ITargetContainer"/>.  Usually, behaviours that are implemented through this
    /// interface are typically used to customise how the underlying <see cref="ITarget"/> registrations are stored
    /// and retrieved in the target container.
    /// 
    /// Functionality such as enumerable resolving, and the default <see cref="IMemberBindingBehaviour"/> to be
    /// used by <see cref="ConstructorTarget"/> are configured by instances of this interface.</remarks>
    /// <seealso cref="GlobalBehaviours.TargetContainerBehaviour"/>
    public interface ITargetContainerBehaviour
    {
        /// <summary>
        /// Called to attach this behaviour to the given <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to which the behaviour is to be attached - will not be 
        /// null when called by the framework.</param>
        void Attach(ITargetContainer targets);
    }
}
