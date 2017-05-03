using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainerBehaviour"/> which contains zero or or more other 
    /// <see cref="ITargetContainerBehaviour"/> objects.  Behaviours can depend on other behaviours, and this 
    /// collection ensures that they are applied in the correct order.
    /// </summary>
    /// <seealso cref="ContainerBehaviourCollection"/>
    public class TargetContainerBehaviourCollection 
        : DependantCollection<ITargetContainerBehaviour>, ITargetContainerBehaviour
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerBehaviourCollection"/> type
        /// </summary>
        public TargetContainerBehaviourCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerBehaviourCollection"/> type, using the passed
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public TargetContainerBehaviourCollection(IEnumerable<ITargetContainerBehaviour> behaviours)
            : base(behaviours)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerBehaviourCollection"/> type, using the passed 
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public TargetContainerBehaviourCollection(params ITargetContainerBehaviour[] behaviours)
            : this((IEnumerable<ITargetContainerBehaviour>)behaviours)
        {

        }

        /// <summary>
        /// Applies each behaviour in the collection to the passed <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container to which the behaviours in this collection are to be attached.</param>
        /// <remarks>The implementation runs through each behaviour that has been added to the collection, in dependency 
        /// order, calling its <see cref="ITargetContainerBehaviour.Attach(ITargetContainer)"/> method, passing the 
        /// <paramref name="targets"/> to each.</remarks>
        public void Attach(ITargetContainer targets)
        {
            foreach (var behaviour in Ordered)
            {
                behaviour.Attach(targets);
            }
        }
    }
}
