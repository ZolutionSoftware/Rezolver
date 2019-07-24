// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver
{
    /// <summary>
    /// A configuration callback for instances of <see cref="IRootTargetContainer"/>.
    /// </summary>
    /// <remarks>
    /// Clearly, this callback interface can be used to perform any action on an <see cref="IRootTargetContainer"/>,
    /// but the intention is to use it either to pre-register targets or target containers for specific types,
    /// or to set options on a target container.
    ///
    /// The automatic injection of enumerables, for example, is enabled by applying the <see cref="Configuration.InjectEnumerables"/>
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
    /// <seealso cref="Configuration.InjectEnumerables"/>
    /// <seealso cref="Configuration.InjectResolveContext"/>
    public interface ITargetContainerConfig
    {
        /// <summary>
        /// Called to apply this configuration to the given <paramref name="rootTargets"/>.
        /// </summary>
        /// <param name="rootTargets">The target container to which the configuration is to be applied - will not be
        /// null when called by the framework.</param>
        void Configure(IRootTargetContainer rootTargets);
    }

    /// <summary>
    /// Marker interface for an <see cref="ITargetContainerConfig"/> which is responsible for configuring a specific type
    /// of object/behaviour/service/option. (Determined by <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">Implementation-dependent.  Type of object that is configured/set/registered by this config.</typeparam>
    /// <remarks>As with <see cref="IContainerConfig{T}"/>, this marker interface is included specifically to provide a convenient way
    /// to express dependencies for configuration objects which depend on, or which must be configured after, others of a specific type.
    ///
    /// The type parameter is completely free-form - it could be a specific service type, an option type
    /// (see <see cref="Configuration.Configure{TOption}"/>), or something else entirely.
    ///
    /// Use of this interface is entirely optional; and you'll only implement it yourself (as with <see cref="ITargetContainerConfig"/>)
    /// if you are extending Rezolver.</remarks>
    /// <seealso cref="IContainerConfig{TContainerService}"/>
    public interface ITargetContainerConfig<T> : ITargetContainerConfig
    {
    }
}
