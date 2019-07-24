// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Interface for the root-most target container, which typically 'owns' all the top-level registrations
    /// required by an application.
    ///
    /// This builds on the <see cref="ITargetContainer"/> interface by adding the <see cref="ICovariantTypeIndex"/>
    /// interface too, as well as events which allow an application to listen for target and target container registrations
    /// within the root.
    /// </summary>
    public interface IRootTargetContainer : ITargetContainer, ICovariantTypeIndex
    {
        /// <summary>
        /// Raised when a target has been registered in this root container (possibly as a child within a
        /// target container that was previously registered).
        /// </summary>
        /// <remarks>
        /// Please note - this event is only guaranteed to fire for targets that are
        /// directly registered through this instance.</remarks>
        event EventHandler<Events.TargetRegisteredEventArgs> TargetRegistered;
        /// <summary>
        /// Raised when a child target container (e.g. a <see cref="DecoratingTargetContainer"/>)
        /// is registered in this root container, or when any container is automatically created and
        /// registered in this root container.
        /// </summary>
        /// <remarks>Please note - this event is only guaranteed to fire for target containers that are
        /// directly registered through this instance.</remarks>
        event EventHandler<Events.TargetContainerRegisteredEventArgs> TargetContainerRegistered;

        /// <summary>
        /// Creates an instance of <see cref="ITargetContainer"/> whose <see cref="ITargetContainer.Root"/>
        /// will be set to this instance and which will ultimately own all registrations for the given
        /// <paramref name="forContainerRegistrationType"/>.
        /// 
        /// Note - the container is not automatically registered - it can be registered with a call to 
        /// this instance's <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> implementation.
        /// </summary>
        /// <param name="forContainerRegistrationType"></param>
        /// <returns></returns>
        ITargetContainer CreateTargetContainer(Type forContainerRegistrationType);

        /// <summary>
        /// Gets the type under which the <see cref="ITargetContainer"/> that will 'own' registrations
        /// for the given <paramref name="serviceType"/> will be registered in this root container.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <remarks>Use of this API is exclusive to code that is extending Rezolver.
        /// 
        /// All targets that are registered in an <see cref="IRootTargetContainer"/> are ultimately stored
        /// inside another <see cref="ITargetContainer"/> that will be registered with the
        /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> API and retrieved
        /// with the <see cref="ITargetContainer.FetchContainer(Type)"/> methods.  When registering
        /// targets, both this method and the <see cref="CreateTargetContainer(Type)"/> methods are used
        /// to ensure that the correct <see cref="ITargetContainer"/> is created to hold a given target.
        /// 
        /// This method is specifically used to identify the type that should be used to fetch a target 
        /// container (or indeed register it) for a given service type.  Most of the time, the two types
        /// are the same; however, there are special cases where they're not.
        /// 
        /// For example, in the standard implementation of this interface (<see cref="TargetContainer"/>),
        /// if you pass a generic type to this method, it will return the definition of that generic type - 
        /// which then allows the <see cref="GenericTargetContainer"/> class to ultimately 'own' all the 
        /// registrations against all generic types - open or closed.
        /// 
        /// Therefore, the system uses this method both to locate existing target containers and when 
        /// auto-creating and registering new target containers to hold new registrations.</remarks>
        Type GetContainerRegistrationType(Type serviceType);
    }
}