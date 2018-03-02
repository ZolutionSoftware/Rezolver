// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Interface for an object which handles an event of a particular type (<typeparamref name="TEvent"/>)
    /// raised from an <see cref="ITargetContainer"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being handled.</typeparam>
    public interface ITargetContainerEventHandler<in TEvent>
    {
        /// <summary>
        /// Fires the event handler for the event <paramref name="e"/> raised from the
        /// target container <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The target container which is raising the event</param>
        /// <param name="e">The event payload.</param>
        void Handle(ITargetContainer source, TEvent e);
    }
}
