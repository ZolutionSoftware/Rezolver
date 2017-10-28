using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> which contains zero or or more other 
    /// <see cref="ITargetContainerConfig"/> objects.  Configurations can depend on others, and this 
    /// collection ensures that they are applied in the correct order.
    /// </summary>
    /// <seealso cref="CombinedContainerConfig"/>
    public class CombinedTargetContainerConfig 
        : DependantCollection<ITargetContainerConfig>, ITargetContainerConfig
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedTargetContainerConfig"/> type
        /// </summary>
        public CombinedTargetContainerConfig()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedTargetContainerConfig"/> type, using the passed
        /// configurations to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public CombinedTargetContainerConfig(IEnumerable<ITargetContainerConfig> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedTargetContainerConfig"/> type, using the passed 
        /// configurations to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public CombinedTargetContainerConfig(params ITargetContainerConfig[] configs)
            : this((IEnumerable<ITargetContainerConfig>)configs)
        {

        }

        /// <summary>
        /// Creates a clone of this combined configuration collection and returns it.
        /// </summary>
        /// <returns>A new instance of <see cref="CombinedTargetContainerConfig"/> whose items are identical
        /// to the collection on which the method is called.</returns>
        public CombinedTargetContainerConfig Clone() => base.Clone<CombinedTargetContainerConfig>();

        /// <summary>
        /// Applies each configuration in this collection to the passed <paramref name="targets"/> <see cref="ITargetContainer"/>.
        /// </summary>
        /// <param name="targets">The target container to which the configurations in this collection are to be applied.</param>
        /// <remarks>The implementation runs through each configuration that has been added to the collection, in dependency 
        /// order, calling its <see cref="ITargetContainerConfig.Configure(IRootTargetContainer)"/> method.</remarks>
        public void Configure(IRootTargetContainer targets)
        {
            foreach (var config in Ordered)
            {
                config.Configure(targets);
            }
        }
    }
}
