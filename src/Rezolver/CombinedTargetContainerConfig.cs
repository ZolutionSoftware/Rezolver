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
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public CombinedTargetContainerConfig(IEnumerable<ITargetContainerConfig> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedTargetContainerConfig"/> type, using the passed 
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public CombinedTargetContainerConfig(params ITargetContainerConfig[] configs)
            : this((IEnumerable<ITargetContainerConfig>)configs)
        {

        }

        /// <summary>
        /// Applies each behaviour in the collection to the passed <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to which the behaviours in this collection are to be attached.</param>
        /// <remarks>The implementation runs through each behaviour that has been added to the collection, in dependency 
        /// order, calling its <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/> method, passing the 
        /// <paramref name="targets"/> to each.</remarks>
        public void Configure(ITargetContainer targets)
        {
            foreach (var config in Ordered)
            {
                config.Configure(targets);
            }
        }
    }
}
