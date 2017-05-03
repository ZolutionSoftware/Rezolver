using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainerBehaviour"/> which contains zero or or more other <see cref="IContainerBehaviour"/>
    /// objects.  Behaviours can depend on other behaviours, and this collection ensures that they are applied
    /// in the correct order.
    /// </summary>
    /// <seealso cref="TargetContainerBehaviourCollection"/>
    public class ContainerBehaviourCollection : DependantCollection<IContainerBehaviour>, IContainerBehaviour
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type
        /// </summary>
        public ContainerBehaviourCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type, using the passed behaviours
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public ContainerBehaviourCollection(IEnumerable<IContainerBehaviour> behaviours)
            : base(behaviours)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type, using the passed behaviours
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public ContainerBehaviourCollection(params IContainerBehaviour[] behaviours)
            : this((IEnumerable<IContainerBehaviour>)behaviours)
        { 

        }

        /// <summary>
        /// Applies the behaviours in this collection to the passed <paramref name="container"/> and 
        /// <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the behaviours are being attached.</param>
        /// <param name="targets">The target container used by the <paramref name="container"/> for its registrations.</param>
        /// <remarks>The implementation runs through each behaviour that has been added to the collection, in dependency 
        /// order, calling its <see cref="IContainerBehaviour.Attach(IContainer, ITargetContainer)"/> method.</remarks>
        public void Attach(IContainer container, ITargetContainer targets)
        {
            foreach(var behaviour in Ordered)
            {
                behaviour.Attach(container, targets);
            }
        }
    }
}
