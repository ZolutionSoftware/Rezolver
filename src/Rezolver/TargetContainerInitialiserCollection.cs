using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainerBehaviour"/> which contains zero or more other, possible interdependent,
    /// initialisers.  When <see cref="Attach(ITargetContainer)"/> is called, the initialisers are applied in
    /// dependency order (since initialisers such as <see cref="ITargetContainerBehaviour"/> and 
    /// <see cref="IContainerBehaviour"/> can have dependencies on others).
    /// </summary>
    public class TargetContainerInitialiserCollection 
        : DependantCollection<ITargetContainerBehaviour>, ITargetContainerBehaviour
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerInitialiserCollection"/> type
        /// </summary>
        public TargetContainerInitialiserCollection()
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerInitialiserCollection"/> type, using the passed initialisers
        /// to seed the underlying collection.
        /// </summary>
        /// <param name="initialisers">The initialisers to be added to the collection on construction.</param>
        public TargetContainerInitialiserCollection(IEnumerable<ITargetContainerBehaviour> initialisers)
            : base(initialisers)
        {

        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainerInitialiserCollection"/> type, using the passed initialisers
        /// to seed the underlying collection.
        /// </summary>
        /// <param name="initialisers">The initialisers to be added to the collection on construction.</param>
        public TargetContainerInitialiserCollection(params ITargetContainerBehaviour[] initialisers)
            : this((IEnumerable<ITargetContainerBehaviour>)initialisers)
        {

        }

        /// <summary>
        /// Runs through each initialiser that has been added to the collection, in dependency order, calling its
        /// <see cref="ITargetContainerBehaviour.Attach(ITargetContainer)"/> method.
        /// </summary>
        /// <param name="targets">The target container to be initialised.</param>
        public void Attach(ITargetContainer targets)
        {
            foreach (var behaviour in Ordered)
            {
                behaviour.Attach(targets);
            }
        }
    }
}
