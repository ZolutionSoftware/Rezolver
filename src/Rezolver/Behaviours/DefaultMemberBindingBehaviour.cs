using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Behaviours
{
    /// <summary>
    /// An <see cref="IContainerBehaviour"/> which can be used to set a container-wide default 
    /// <see cref="IMemberBindingBehaviour"/> for all 
    /// </summary>
    public class DefaultMemberBindingBehaviour : IContainerBehaviour<IMemberBindingBehaviour>
    {
        private IMemberBindingBehaviour _memberBindingBehaviour;
        /// <summary>
        /// Constructs a new instance of the <see cref="DefaultMemberBindingBehaviour"/> for the 
        /// passed <see cref="IMemberBindingBehaviour"/>.
        /// </summary>
        /// <param name="memberBindingBehaviour">Required - the <see cref="IMemberBindingBehaviour"/> that is to be
        /// used as the default for the container to which this behaviour is attached.</param>
        public DefaultMemberBindingBehaviour(IMemberBindingBehaviour memberBindingBehaviour)
        {
            _memberBindingBehaviour = memberBindingBehaviour ?? throw new ArgumentNullException(nameof(memberBindingBehaviour));
        }

        /// <summary>
        /// Implementation of <see cref="IContainerBehaviour.Attach(IContainer, ITargetContainer)"/>.
        /// 
        /// After it's attached, the container will resolve <see cref="IMemberBindingBehaviour"/> to
        /// the behaviour that was passed to this object's constructor.
        /// </summary>
        /// <param name="container">The container to which this behaviour is to be attached.</param>
        /// <param name="targets">The target container which provides the registrations for the <paramref name="container"/></param>
        public void Attach(IContainer container, ITargetContainer targets)
        {
            // just register the object in the target container
            targets.RegisterObject(_memberBindingBehaviour);
        }
    }
}
