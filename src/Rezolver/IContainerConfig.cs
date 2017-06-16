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
    /// A configuration instance can be passed to any of the provided <see cref="ContainerBase"/>-derived types on 
    /// construction (see the <see cref="ContainerBase.ContainerBase(IContainerConfig, ITargetContainer)"/> 
    /// constructor).  If one is not passed then a default is used.
    /// 
    /// The built-in config implementations register services or set options in the target container that 
    /// is used by the container to change or add functionality.
    /// </remarks>
    /// <seealso cref="ITargetContainerConfig"/>
    /// <seealso cref=""/>
    public interface IContainerConfig
    {
        /// <summary>
        /// Attaches the behaviour to the <paramref name="container"/> and/or its <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the behaviour is to be attached.</param>
        /// <param name="targets">The <see cref="ITargetContainer"/> for the <paramref name="container"/> to
        /// which the behaviour is to be attached.</param>
        void Attach(IContainer container, ITargetContainer targets);
    }

    /// <summary>
    /// Represents a behaviour for an <see cref="IContainer"/> which configures/extends or supplies a particular 
    /// service (and potentially services related to <typeparamref name="TContainerService"/>) for the container.
    /// </summary>
    /// <typeparam name="TContainerService">Type of service used by the container that is managed by this behaviour.</typeparam>
    /// <remarks>
    /// The type <typeparamref name="TContainerService"/> is not exclusive (i.e. the behaviour is free to configure 
    /// other services) - it's just a hint to the framework as to the intention of the behaviour.  It also provides a 
    /// commmon way to identify implementations of the same type of behaviour in a collection.  E.g. there might be many 
    /// alternative behaviours for <c>IContainerBehaviour&lt;Foo&gt;</c> and you might have a piece of code which 
    /// replaces any of those with a different one.
    /// 
    /// Some types passed for <typeparamref name="TContainerService"/> are well-known to Rezolver and carry expectations 
    /// as to what they will do (and thus will undergo validation after being run).  One example is 
    /// <see cref="ITargetCompiler"/> - an <c>IContainerBehaviour&lt;ITargetCompiler&gt;</c> is expected to ensure that
    /// a compiler can be resolved from the container after the behaviour has been attached.
    /// 
    /// In closing, direct use of this stuff is not normally required for standard applications - all of the standard
    /// behaviours hide the complexity behind extension methods and/or friendly behaviour types.
    /// 
    /// You'll only need to use it directly if you're extending some of the more advanced parts of the framework.</remarks>
    public interface IContainerConfig<TContainerService> : IContainerConfig
    {

    }
}