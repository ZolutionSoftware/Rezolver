// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver.Events
{
    /// <summary>
    /// Represents the arguments passed to the <see cref="IRootTargetContainer.TargetContainerRegistered"/>
    /// event exposed by the <see cref="IRootTargetContainer"/> interface.
    /// </summary>
    public class TargetContainerRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// The target container that was registered.
        /// </summary>
        public ITargetContainer Container { get; }

        /// <summary>
        /// The type against which the target container was registered.  Note that this type
        /// could be an open generic.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerRegisteredEventArgs"/>
        /// </summary>
        /// <param name="container">The target container that was registered</param>
        /// <param name="type">The type against which the target container was registered</param>
        public TargetContainerRegisteredEventArgs(ITargetContainer container, Type type)
        {
            Container = container;
            Type = type;
        }
    }
}