using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Events
{
    /// <summary>
    /// An event fired from an <see cref="ITargetContainer"/> when a <see cref="Target"/> registration is added to
    /// the target container against a particular <see cref="ServiceType"/>.
    /// </summary>
    /// <remarks>To receive notifications of this event from your root <see cref="ITargetContainer"/>, add a handler 
    /// with the <see cref="TargetContainerEventExtensions.RegisterEventHandler{TEvent}(ITargetContainer, ITargetContainerEventHandler{TEvent})"/>
    /// extension method.
    /// 
    /// Note that if you're using the <see cref="TargetContainer"/> type as your root target container (the default for
    /// container types like <see cref="Container"/> and <see cref="ScopedContainer"/>), then you only need to subscribe to
    /// its event and you will receive notifications for all targets added to it, or any of its child container types.</remarks>
    public sealed class TargetRegisteredEvent
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TargetRegisteredEvent"/> type
        /// </summary>
        /// <param name="target">Required.  The target that was registered</param>
        /// <param name="serviceType">Required.  The service type against which the <paramref name="target"/> was registered.</param>
        public TargetRegisteredEvent(ITarget target, Type serviceType)
        {
            Target = target;
            ServiceType = serviceType;
        }

        /// <summary>
        /// The target that was registered.
        /// </summary>
        public ITarget Target { get; }
        /// <summary>
        /// The service type against which the <see cref="Target"/> was registered.
        /// </summary>
        public Type ServiceType { get; }
    }
}
