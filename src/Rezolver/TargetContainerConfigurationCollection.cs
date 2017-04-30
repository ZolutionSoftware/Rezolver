using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainerConfiguration"/> which contains zero or more other, possible interdependent,
    /// configurations.  When <see cref="Configure(ITargetContainer)"/> is called, the configurations are applied in
    /// dependency order (since configuration objects such as <see cref="ITargetContainerConfiguration"/> and 
    /// <see cref="IContainerConfiguration"/> can have dependencies on others).
    /// </summary>
    public class TargetContainerConfigurationCollection 
        : DependantCollection<ITargetContainerConfiguration>, ITargetContainerConfiguration
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigurationCollection"/> type
        /// </summary>
        public TargetContainerConfigurationCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigurationCollection"/> type, using the passed configurations
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The configurations to be added to the collection on construction.</param>
        public TargetContainerConfigurationCollection(IEnumerable<ITargetContainerConfiguration> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigurationCollection"/> type, using the passed configurations
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The configurations to be added to the collection on construction.</param>
        public TargetContainerConfigurationCollection(params ITargetContainerConfiguration[] configs)
            : this((IEnumerable<ITargetContainerConfiguration>)configs)
        {

        }

        /// <summary>
        /// Runs through each configuration that has been added to the collection, in dependency order, calling its
        /// <see cref="ITargetContainerConfiguration.Configure(ITargetContainer)"/> method.
        /// </summary>
        /// <param name="targets">The target container to be configured.</param>
        public void Configure(ITargetContainer targets)
        {
            foreach (var behaviour in Ordered)
            {
                behaviour.Configure(targets);
            }
        }
    }
}
