using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Behaviours
{
    /// <summary>
    /// An <see cref="ITargetContainerBehaviour"/> which enables automatic handling of fetching
    /// targets for <see cref="IEnumerable{T}"/> based on all the targets registered for a given <c>T</c>
    /// in an <see cref="ITargetContainer"/>.
    /// </summary>
    /// <remarks>This behaviour is added to the default <see cref="GlobalBehaviours.TargetContainerBehaviour"/>.
    /// 
    /// If this behaviour is not attached to an <see cref="ITargetContainer"/> instance, then only explicitly
    /// registered enumerables will be able to be resolved from any <see cref="IContainer"/> built from that 
    /// target container.</remarks>
    public class AutoEnumerableBehaviour : ITargetContainerBehaviour
    {
        /// <summary>
        /// The one and only instance of the <see cref="AutoEnumerableBehaviour"/> type.
        /// </summary>
        public static AutoEnumerableBehaviour Instance { get; } = new AutoEnumerableBehaviour();
        private AutoEnumerableBehaviour()
        {
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainerBehaviour.Attach(ITargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        public void Attach(ITargetContainer targets)
        {
            targets.MustNotBeNull(nameof(targets));
            if(targets.FetchContainer(typeof(IEnumerable<>)) == null)
                targets.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(targets));
        }
    }
}
