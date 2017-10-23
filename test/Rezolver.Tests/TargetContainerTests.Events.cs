using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Events;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetRegisteredEventHandler : ITargetContainerEventHandler<TargetRegisteredEvent>
    {
        public List<(ITargetContainer, TargetRegisteredEvent)> HandledEvents { get; } = new List<(ITargetContainer, TargetRegisteredEvent)>();

        public (ITargetContainer, TargetRegisteredEvent) LastEvent {  get { return HandledEvents[HandledEvents.Count - 1]; } }
        public void Handle(ITargetContainer source, TargetRegisteredEvent e)
        {
            HandledEvents.Add((source, e));
        }
    }

    public class TargetContainer_EventsTests
    {
        [Fact]
        public void ShouldReceiveNotificationOfTargetAdded()
        {
            // Arrange
            ITargetContainer targetContainer = new TargetContainer();
            var handler = new TargetRegisteredEventHandler();
            targetContainer.RegisterEventHandler(handler);

            // Act
            var target = Target.ForObject(1);
            targetContainer.Register(target);

            // Assert
            Assert.Same(targetContainer, handler.LastEvent.Item1);
            Assert.Same(target, handler.LastEvent.Item2.Target);
            Assert.Equal(typeof(int), handler.LastEvent.Item2.ServiceType);
        }

        // So: the idea is to use event handlers - defined as options in the target container - as a way to 'tack-on' the necessary information
        // required to perform explicit ordering in Enumerables.  The various target container types will fire events for registration of new targets
        // and target containers - and then the enumerable target container can 'listen' for those, and assign each new target a sequence number (using
        // the ReferenceComparer that's already being used for the dependency graph implementation) , which then allows it later to order the targets
        // when building the enumerable :)
    }
}
