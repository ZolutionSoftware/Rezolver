using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public sealed class TestEvent
    {
        public string Message { get; }
        public TestEvent(string message)
        {
            Message = message;
        }
    }

    public interface ITargetContainerEventHandler<in TEvent>
    {
        void Handle(ITargetContainer source, TEvent e);
    }

    public class TestEventHandler1 : ITargetContainerEventHandler<TestEvent>
    {
        public List<string> ReceivedMessages { get; } = new List<string>();

        public void Handle(ITargetContainer source, TestEvent e) => ReceivedMessages.Add(e.Message);
    }

    public class TestEventHandler2 : ITargetContainerEventHandler<TestEvent>
    {
        public List<TestEvent> ReceivedEvents { get; } = new List<TestEvent>();

        public void Handle(ITargetContainer source, TestEvent e) => ReceivedEvents.Add(e);
    }
    public static class TargetContainerEventExtensions
    {
        public static void RegisterEventHandler<TEvent>(this ITargetContainer container, ITargetContainerEventHandler<TEvent> handler)
        {
            container.SetOption(handler);
        }

        public static IEnumerable<ITargetContainerEventHandler<TEvent>> GetEventHandlers<TEvent>(this ITargetContainer container, TEvent e = default(TEvent))
        {
            return container.GetOptions<ITargetContainerEventHandler<TEvent>>();
        }
    }

    public class TargetContainer_EventsTests
    {
        [Fact]
        public void ShouldGetEventHandlers()
        {
            // Arrange
            ITargetContainer targetContainer = new TargetContainer();
            var handler1 = new TestEventHandler1();
            var handler2 = new TestEventHandler2();

            targetContainer.RegisterEventHandler(handler1);
            targetContainer.RegisterEventHandler(handler2);

            // Act
            var handlers = targetContainer.GetEventHandlers<TestEvent>();

            // Assert
            Assert.Equal(new ITargetContainerEventHandler<TestEvent>[] { handler1, handler2 }, handlers);
        }

        // So: the idea is to use event handlers - defined as options in the target container - as a way to 'tack-on' the necessary information
        // required to perform explicit ordering in Enumerables.  The various target container types will fire events for registration of new targets
        // and target containers - and then the enumerable target container can 'listen' for those, and assign each new target a sequence number (using
        // the ReferenceComparer that's already being used for the dependency graph implementation) , which then allows it later to order the targets
        // when building the enumerable :)
    }
}
