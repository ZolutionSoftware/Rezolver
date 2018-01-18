using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rezolver.Events;
using Xunit;

namespace Rezolver.Tests
{
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
    }
}
