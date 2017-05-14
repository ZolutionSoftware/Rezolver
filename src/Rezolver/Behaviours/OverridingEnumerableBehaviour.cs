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
    /// Note that this class is not an <see cref="ITargetContainerBehaviour"/> like the <see cref="AutoEnumerableBehaviour"/>.
    /// 
    /// It is an <see cref="IContainerBehaviour"/> because it only applies to instances of <see cref="OverridingContainer"/>.</remarks>
    public class OverridingEnumerableBehaviour : IContainerBehaviour
    {
        public static OverridingEnumerableBehaviour Instance { get; } = new OverridingEnumerableBehaviour();

        private OverridingEnumerableBehaviour() { }
        public void Attach(IContainer container, ITargetContainer targets)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            targets.RegisterContainer(typeof(IEnumerable<>), 
                new ConcatenatingEnumerableContainer(container, targets));
        }
    }
}
