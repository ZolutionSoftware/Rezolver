using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Provides configuration for <see cref="ITargetContainer"/> instances.
    /// </summary>
    /// <remarks>This abstraction provides a way to apply common setup procedures to <see cref="ITargetContainer"/>
    /// objects which are then used by containers and during compilation - typically by registering targets or 
    /// target containers within it.
    /// 
    /// While similar to <see cref="IContainerConfiguration"/>, this is specifically intended to be used when
    /// creating any <see cref="ITargetContainer"/>; whereas the other interface is for when a container is created
    /// which also uses <see cref="ITargetContainer"/>.
    /// 
    /// Functionality such as enumerable resolving, and the default <see cref="IMemberBindingBehaviour"/> to be
    /// used by <see cref="ConstructorTarget"/> are configured through this interface - which is expected to
    /// register services into the target container passed to <see cref="Configure(ITargetContainer)"/> which can
    /// later be resolved from the <see cref="IContainer"/> which is available at compile time.</remarks>
    public interface ITargetContainerConfiguration
    {
        /// <summary>
        /// Called to configure the given <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to be configured - will not be null when called by the framework.</param>
        void Configure(ITargetContainer targets);
    }
}
