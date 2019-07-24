// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Events
{
    /// <summary>
    /// Contains the arguments for the <see cref="IRootTargetContainer.TargetRegistered"/> event exposed
    /// by the <see cref="IRootTargetContainer"/>
    /// </summary>
    public class TargetRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// The target that was registered
        /// </summary>
        public ITarget Target { get; }

        /// <summary>
        /// The type against which the target was registered.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetRegisteredEventArgs"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        public TargetRegisteredEventArgs(ITarget target, Type type)
        {
            Target = target;
            Type = type;
        }
    }
}