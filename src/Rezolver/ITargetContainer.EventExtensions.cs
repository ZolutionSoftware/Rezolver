using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Contains the extension <see cref="RegisterEventHandler{TEvent}(ITargetContainer, ITargetContainerEventHandler{TEvent})"/> 
    /// which allows a caller to receive notifications about events happening inside a particular target container.
    /// </summary>
    public static class TargetContainerEventExtensions
    {
        /// <summary>
        /// Adds an event handler to the target container for the event type <typeparamref name="TEvent"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event that is to be handled by the handler.</typeparam>
        /// <param name="targets">Required.  The target container to which the event subscription is being added.</param>
        /// <param name="handler">Required.  The handler that is to be invoked when the <paramref name="targets"/>
        /// target container raises an event of type <typeparamref name="TEvent"/></param>
        /// <remarks>Event handlers are implemented as registrations inside the <paramref name="targets"/> target
        /// container behind the scenes; as such it is not possible to remove an event handler after adding it.</remarks>
        public static void RegisterEventHandler<TEvent>(this ITargetContainer targets, ITargetContainerEventHandler<TEvent> handler)
        {
            (targets ?? throw new ArgumentNullException(nameof(targets)))
                .SetOption(handler ?? throw new ArgumentNullException(nameof(handler)));
        }

        internal static IEnumerable<ITargetContainerEventHandler<TEvent>> GetEventHandlers<TEvent>(this ITargetContainer container, TEvent e = default(TEvent))
        {
            return container.GetOptions<ITargetContainerEventHandler<TEvent>>();
        }

        internal static void RaiseEvent<TEvent>(this ITargetContainer container, TEvent e)
        {
            foreach(var handler in GetEventHandlers(container, e))
            {
                handler.Handle(container, e);
            }
        }
    }
}
