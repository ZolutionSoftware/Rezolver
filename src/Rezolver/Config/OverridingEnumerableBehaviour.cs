using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Behaviours
{
    /// <summary>
    /// This extends the behaviour supplied by the AutoEnumerableBehaviour to the <see cref="OverridingContainer"/>
    /// class by chaining the enumerable from the overridden container to the enumerable produced by the overriding container.
    /// </summary>
    /// <remarks>
    /// Note that this class is not an <see cref="ITargetContainerConfig"/> like the <see cref="AutoEnumerableConfig"/>,
    /// instead it is an <see cref="IContainerBehaviour"/> because it's only relevant for instances of <see cref="OverridingContainer"/>.</remarks>
    public sealed class OverridingEnumerableBehaviour : IContainerBehaviour
    {
        /// <summary>
        /// The one and only instance of the <see cref="OverridingEnumerableBehaviour"/>
        /// </summary>
        public static OverridingEnumerableBehaviour Instance { get; } = new OverridingEnumerableBehaviour();

        private OverridingEnumerableBehaviour() { }

        /// <summary>
        /// Attaches this behaviour to the container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="targets"></param>
        public void Attach(IContainer container, ITargetContainer targets)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            targets.RegisterContainer(typeof(IEnumerable<>), 
                new ConcatenatingEnumerableContainer(container, targets));
        }
    }
}
