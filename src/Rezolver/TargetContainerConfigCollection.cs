using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> which contains zero or or more other 
    /// <see cref="ITargetContainerConfig"/> objects.  Behaviours can depend on other behaviours, and this 
    /// collection ensures that they are applied in the correct order.
    /// </summary>
    /// <seealso cref="ContainerBehaviourCollection"/>
    public class TargetContainerConfigCollection 
        : DependantCollection<ITargetContainerConfig>, ITargetContainerConfig
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigCollection"/> type
        /// </summary>
        public TargetContainerConfigCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigCollection"/> type, using the passed
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public TargetContainerConfigCollection(IEnumerable<ITargetContainerConfig> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerConfigCollection"/> type, using the passed 
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The behaviours to be added to the collection on construction.</param>
        public TargetContainerConfigCollection(params ITargetContainerConfig[] configs)
            : this((IEnumerable<ITargetContainerConfig>)configs)
        {

        }

        /// <summary>
        /// Applies each behaviour in the collection to the passed <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to which the behaviours in this collection are to be attached.</param>
        /// <remarks>The implementation runs through each behaviour that has been added to the collection, in dependency 
        /// order, calling its <see cref="ITargetContainerConfig.Apply(ITargetContainer)"/> method, passing the 
        /// <paramref name="targets"/> to each.</remarks>
        public void Apply(ITargetContainer targets)
        {
            foreach (var config in Ordered)
            {
                config.Apply(targets);
            }
        }
    }
}
