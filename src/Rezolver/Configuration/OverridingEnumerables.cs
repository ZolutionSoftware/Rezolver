using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// When applied to an <see cref="OverridingContainer"/> whose target container has been configured to 
    /// enable automatically injected enumerables (via the <see cref="AutoEnumerables"/> configuration callback),
    /// then this will extend enumerable support in the <see cref="OverridingContainer"/> to construct enumerables
    /// made up of a combination of all services in the overriding container, in addition to those from the 
    /// base container.
    /// </summary>
    /// <remarks>
    /// Note that this class is not an <see cref="ITargetContainerConfig"/> like the <see cref="AutoEnumerables"/>,
    /// instead it is an <see cref="IContainerConfig"/> because it's only relevant for instances of <see cref="OverridingContainer"/>.</remarks>
    public sealed class OverridingEnumerables : IContainerConfig
    {
        /// <summary>
        /// The one and only instance of the <see cref="OverridingEnumerables"/>
        /// </summary>
        public static OverridingEnumerables Instance { get; } = new OverridingEnumerables();

        private OverridingEnumerables() { }

        /// <summary>
        /// Attaches this behaviour to the container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="targets"></param>
        public void Configure(IContainer container, ITargetContainer targets)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            if (container is OverridingContainer && targets.GetOption(Options.EnableAutoEnumerable.Default))
            {
                targets.RegisterContainer(typeof(IEnumerable<>),
                    new ConcatenatingEnumerableContainer(container, targets));
            }
        }
    }
}
