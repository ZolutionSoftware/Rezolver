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
            IRootTargetContainer targetContainer = new TargetContainer();
            List<(IRootTargetContainer, Events.TargetRegisteredEventArgs)> allEvents = new List<(IRootTargetContainer, TargetRegisteredEventArgs)>();
            targetContainer.TargetRegistered += (o, e) =>
            {
                allEvents.Add((o as IRootTargetContainer, e));
            };

            // Act
            var target = Target.ForObject(1);
            targetContainer.Register(target);

            // Assert
            Assert.NotEmpty(allEvents);
            var lastEvent = allEvents[allEvents.Count - 1];
            Assert.Same(targetContainer, lastEvent.Item1);
            Assert.Same(target, lastEvent.Item2.Target);
            Assert.Equal(typeof(int), lastEvent.Item2.Type);
        }

        // So: the idea is to use event handlers - defined as options in the target container - as a way to 'tack-on' the necessary information
        // required to perform explicit ordering in Enumerables.  The various target container types will fire events for registration of new targets
        // and target containers - and then the enumerable target container can 'listen' for those, and assign each new target a sequence number (using
        // the ReferenceComparer that's already being used for the dependency graph implementation) , which then allows it later to order the targets
        // when building the enumerable :)
    }
}
