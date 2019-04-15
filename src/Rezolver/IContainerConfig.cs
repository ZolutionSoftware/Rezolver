// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver
{
    /// <summary>
    /// A configuration callback for instances of <see cref="IContainer"/> (which also use <see cref="ITargetContainer"/>
    /// as the source of their registrations).
    /// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerConfig"/>, this is specifically used
    /// for new container instances - since some configuration (setting of options etc) only applies to
    /// <see cref="IContainer"/> and not <see cref="ITargetContainer"/>.
    ///
    /// A configuration instance can be passed to any of the provided <see cref="Container"/>-derived types on
    /// construction (see the <see cref="Container.Container(IRootTargetContainer, IContainerConfig)"/>
    /// constructor).  If one is not passed then the <see cref="Container.DefaultConfig"/> is used.
    ///
    /// The built-in config implementations register services or set options in the target container passed
    /// to <see cref="Configure(Container, IRootTargetContainer)"/>.
    /// </remarks>
    /// <seealso cref="ITargetContainerConfig"/>
    /// <seealso cref="Configuration.ExpressionCompilation"/>
    /// <seealso cref="Configuration.OverridingEnumerables"/>
    public interface IContainerConfig
    {
        /// <summary>
        /// Performs the configuration represented by this insteance on the <paramref name="container"/> and its
        /// <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the configuration is to be applied.</param>
        /// <param name="targets">The <see cref="ITargetContainer"/> that is being used by the <paramref name="container"/>
        /// for its registrations and options.</param>
        void Configure(Container container, IRootTargetContainer targets);
    }

    /// <summary>
    /// Marker interface for an <see cref="ITargetContainerConfig"/> which is responsible for configuring a specific type
    /// of object/behaviour/service/option for <see cref="IContainer"/> instances. (Determined by <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">Implementation-dependent.  The type of option or service used by the container that is configured
    /// by this configuration object.</typeparam>
    /// <remarks>As with <see cref="ITargetContainerConfig{T}"/>, this marker interface is included specifically to provide
    /// a convenient way to expression dependencies for container-related configuration objects which depend on, or which
    /// must be configured after, others of a specific type.
    ///
    /// The type parameter is completely free-form - it could be a specific service type, an option type, or something else
    /// entirely.
    ///
    /// Use of this interface is entirely optional; and you'll only implement it yourself (as with <see cref="IContainerConfig"/>)
    /// if you are extending Rezolver.
    /// </remarks>
    public interface IContainerConfig<T> : IContainerConfig
    {
    }
}