using Rezolver.Compilation;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Represents a behaviour for an <see cref="IContainer"/> (specifically one which also uses an 
    /// <see cref="ITargetContainer"/> as the source of its registrations).
    /// </summary>
    /// <remarks>While similar to <see cref="ITargetContainerBehaviour"/>, this is specifically used
    /// for new containers.
    /// 
    /// A behaviour can be passed to any of the provided <see cref="ContainerBase"/>-derived types on 
    /// construction (see the <see cref="ContainerBase.ContainerBase(IContainerBehaviour, ITargetContainer)"/> 
    /// constructor).  If one is not passed then a default is used from the <see cref="GlobalBehaviours"/> class.
    /// 
    /// The actual one used depends whether it's an <see cref="OverridingContainer"/> or not: Rezolver has global 
    /// behaviours for the standalone container types (via the <see cref="GlobalBehaviours.ContainerBehaviour"/> 
    /// property) and for overriding containers (via the <see cref="GlobalBehaviours.OverridingContainerBehaviour"/> 
    /// property).
    /// 
    /// 
    /// Behaviours extend <see cref="ITargetContainer"/>-based containers by registering well-known service types
    /// in the target container passed to <see cref="Attach"/> - which is the target container from which the container
    /// itself will retrieve registrations when resolving instances.  The built in <see cref="IContainer"/> classes resolve
    /// these service types (e.g. <see cref="ITargetCompiler"/>) as part of normal operation, so - by providing alternative
    /// or additional registrations - behaviours can extend the underlying container.</remarks>
    /// <seealso cref="ITargetContainerBehaviour"/>
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
    /// <see cref="ITargetCompiler"/> - an <c>IContainerBehaviour&lt;ITargetCompiler&gt;</c> is expected to configure
    /// both an <see cref="ITargetCompiler"/> and an <see cref="ICompileContextProvider"/> and if it doesn't then
    /// an error will occur.
    /// 
    /// In closing, direct use of this stuff is not normally required for standard applications - all of the standard
    /// behaviours hide the complexity behind extension methods and/or friendly behaviour types.</remarks>
    public interface IContainerBehaviour<TContainerService> : IContainerBehaviour
    {

    }
}