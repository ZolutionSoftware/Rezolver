using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Static accessor for the common member binding behaviours provided by Rezolver.
    /// </summary>
    public static class MemberBindingBehaviour
    {
        /// <summary>
        /// A behaviour that binds all writeable properties and fields on an object after construction.
        /// </summary>
        /// <remarks>The implementation is an instance of the <see cref="BindAllMembersBehaviour"/></remarks>
        public static IMemberBindingBehaviour BindAll => BindAllMembersBehaviour.Instance;

        /// <summary>
        /// A behaviour that doesn't bind any properties or fields on an object.
        /// 
        /// This is the container default behaviour - configured via a <see cref="Behaviours.DefaultMemberBindingBehaviour"/>
        /// in the <see cref="GlobalBehaviours.ContainerBehaviour"/>.
        /// </summary>
        /// <remarks>The implementation is an instance of the <see cref="BindNoMembersBehaviour"/></remarks>
        public static IMemberBindingBehaviour BindNone => BindNoMembersBehaviour.Instance;
    }
}
