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
    /// <remarks><see cref="IContainer"/> objects are configured by registering
    /// known services in the <see cref="ITargetContainer"/> that they're built from. Initialisers can also be
    /// interdependent - i.e. initialiser A requires initialiser B in order to work - hence
    /// the use of the <see cref="IDependant"/> interface on the <see cref="IContainerBehaviour"/> interface.
    /// 
    /// This collection implements <see cref="IContainerBehaviour.Attach(IContainer, ITargetContainer)"/> 
    /// by running through all the behaviours that have been added to it, in order of least to most dependant.</remarks>
    public class ContainerBehaviourCollection : DependantCollection<IContainerBehaviour>, IContainerBehaviour
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type
        /// </summary>
        public ContainerBehaviourCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type, using the passed initialisers
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The initialisers to be added to the collection on construction.</param>
        public ContainerBehaviourCollection(IEnumerable<IContainerBehaviour> configs)
            : base(configs)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ContainerBehaviourCollection"/> type, using the passed initialisers
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="configs">The initialisers to be added to the collection on construction.</param>
        public ContainerBehaviourCollection(params IContainerBehaviour[] configs)
            : this((IEnumerable<IContainerBehaviour>)configs)
        { 

        }

        /// <summary>
        /// Runs through each initialiser that has been added to the collection, in dependency order, calling its
        /// <see cref="IContainerBehaviour.Attach(IContainer, ITargetContainer)"/> method.
        /// </summary>
        /// <param name="container">The container being configured</param>
        /// <param name="targets">The target container used by the <paramref name="container"/> for its registrations.</param>
        public void Attach(IContainer container, ITargetContainer targets)
        {
            foreach(var behaviour in Ordered)
            {
                behaviour.Attach(container, targets);
            }
        }
    }
}
