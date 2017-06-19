using Rezolver.Compilation;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// A configuration callback for instances of <see cref="IContainer"/> which also use <see cref="ITargetContainer"/>
    /// as the source of their registrations.
    /// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerConfig"/>, this is specifically used
    /// for new container instances - since some configuration (setting of options etc) only applies to
    /// containers rather than target containers.
    /// 
    /// A configuration instance can be passed to any of the provided <see cref="Container"/>-derived types on 
    /// construction (see the <see cref="Container.Container(ITargetContainer, IContainerConfig)"/> 
    /// constructor).  If one is not passed then a default is used.
    /// 
    /// The built-in config implementations register services or set options in the target container that 
    /// is used by the container to change or add functionality.
    /// </remarks>
    /// <seealso cref="ITargetContainerConfig"/>
    public interface IContainerConfig
    {
        /// <summary>
        /// Attaches the behaviour to the <paramref name="container"/> and/or its <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the behaviour is to be attached.</param>
        /// <param name="targets">The <see cref="ITargetContainer"/> for the <paramref name="container"/> to
        /// which the behaviour is to be attached.</param>
        void Configure(IContainer container, ITargetContainer targets);
    }

    /// <summary>
    /// A config targetted at a particular container service or option.
    /// </summary>
    /// <typeparam name="TContainerService">Type of option or service used by the container that is configured
    /// by this configuration object.</typeparam>
    public interface IContainerConfig<TContainerService> : IContainerConfig
    {

    }
}