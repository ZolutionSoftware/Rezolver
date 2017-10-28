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
        /// Raised when a target has been registered to this root container.
        /// </summary>
        event EventHandler<Events.TargetRegisteredEventArgs> TargetRegistered;
        /// <summary>
        /// Raised when a child target container (e.g. a <see cref="DecoratingTargetContainer"/>)
        /// is registered in this root container.
        /// </summary>
        event EventHandler<Events.TargetContainerRegisteredEventArgs> TargetContainerRegistered;
    }
}