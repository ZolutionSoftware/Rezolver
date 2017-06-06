using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Represents a behaviour that can be attached to <see cref="ITargetContainer"/> instances to
    /// customise how it behaves - typically by registering targets or target containers to the 
    /// passed <see cref="ITargetContainer"/> via its <see cref="ITargetContainer.Register(ITarget, System.Type)"/>
    /// or <see cref="ITargetContainer.RegisterContainer(System.Type, ITargetContainer)"/> methods.
    /// </summary>
    /// <remarks>Behaviours are separate from options (<see cref="OptionsTargetContainerExtensions"/>)
    /// in that options are *read* by the code in the Rezolver framework to alter how certain functionality
    /// works (e.g. enabling/disabling multiple registrations or generic contravariance).
    /// 
    /// Instead, behaviours are typically used to add one or more registrations to an <see cref="ITargetContainer"/>
    /// to extend the types that a container will be able to inject without any further configuration by the application.
    /// 
    /// For example, the automatic injection of enumerables is enabled by attaching the 
    /// <see cref="Behaviours.AutoEnumerableBehaviour"/>.
    /// </remarks>
    /// <seealso cref="GlobalBehaviours.TargetContainerBehaviour"/>
    /// <seealso cref="Behaviours.AutoEnumerableBehaviour"/>
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
