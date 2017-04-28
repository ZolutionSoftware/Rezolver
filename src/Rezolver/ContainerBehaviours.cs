using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainerBehaviour"/> which contains zero or or more other <see cref="IContainerBehaviour"/>
    /// objects.
    /// </summary>
    /// <remarks>Container behaviours extend the functionality of <see cref="IContainer"/> objects by registering
    /// known services in the <see cref="ITargetContainer"/> that they're built from.  Behaviours can also be
    /// interdependent - i.e. behaviour A requires behaviour B to be configured before it can be configured - hence
    /// the use of the <see cref="IDependant"/> interface on the <see cref="IContainerBehaviour"/> interface.
    /// 
    /// This collection implements <see cref="IContainerBehaviour.Configure(IContainer, ITargetContainer)"/> 
    /// by running through all the behaviours that have been added to it, in order of least to most dependant.</remarks>
    public class ContainerBehaviours : DependantCollection<IContainerBehaviour>, IContainerBehaviour
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviours"/> type
        /// </summary>
        public ContainerBehaviours()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviours"/> type, using the passed enumerable
        /// of behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to added to the collection on construction.</param>
        public ContainerBehaviours(IEnumerable<IContainerBehaviour> behaviours)
            : base(behaviours)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviours"/> type, using the passed
        /// behaviours to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to added to the collection on construction.</param>
        public ContainerBehaviours(params IContainerBehaviour[] behaviours)
            : this((IEnumerable<IContainerBehaviour>)behaviours)
        { 

        }

        /// <summary>
        /// Runs through each behaviour that has been added to the collection, in dependency order, calling its
        /// <see cref="IContainerBehaviour.Configure(IContainer, ITargetContainer)"/> method.
        /// </summary>
        /// <param name="container">The container being configured</param>
        /// <param name="targets">The target container used by the <paramref name="container"/> for its registrations.</param>
        public void Configure(IContainer container, ITargetContainer targets)
        {
            foreach(var behaviour in Ordered)
            {
                behaviour.Configure(container, targets);
            }
        }
    }
}
