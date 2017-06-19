using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// A configuration callback for instances of <see cref="ITargetContainer"/>.
    /// </summary>
    /// <remarks>
    /// Clearly, this callback interface can be used to perform any action on an <see cref="ITargetContainer"/>,
    /// but the intention is to use it either to pre-register targets or target containers for specific types,
    /// or to set options on a target container.
    /// 
    /// The automatic injection of enumerables, for example, is enabled by applying the <see cref="Configuration.AutoEnumerables"/>
    /// to a target container.
    /// 
    /// Different target containers also have their own statically available default configuration collections,
    /// of the type <see cref="CombinedTargetContainerConfig"/>, which also implements this interface by
    /// applying multiple configurations to a target container - providing an easy way to combine multiple configurations
    /// as one.  The most commonly used and modified of these is the <see cref="TargetContainer.DefaultConfig"/>
    /// collection.
    /// </remarks>
    /// <seealso cref="TargetContainer.DefaultConfig"/>
    /// <seealso cref="CombinedTargetContainerConfig"/>
    /// <seealso cref="Configuration.AutoEnumerables"/>
    /// <seealso cref="Configuration.InjectResolveContext"/>
    public interface ITargetContainerConfig
    {
        /// <summary>
        /// Called to apply this configuration to the given <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to which the configuration is to be applied - will not be 
        /// null when called by the framework.</param>
        void Configure(ITargetContainer targets);
    }
}
