using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Holds the default member binding behaviour for the <see cref="ConstructorTarget"/>
    /// </summary>
    public static class MemberBindingBehaviour
    {
        private static IMemberBindingBehaviour _default;

        /// <summary>
        /// The default <see cref="IMemberBindingBehaviour"/> - set to <see cref="BindNoMembersBehaviour"/>
        /// by default.  Note that this property is used indirectly by the <see cref="ConstructorTarget"/>
        /// when resolving objects - in that an <see cref="IMemberBindingBehaviour"/> is resolved from the
        /// current <see cref="IContainer"/> during compilation - and that 
        /// </summary>
        public static IMemberBindingBehaviour Default
        {
            get
            {
                return _default;
            }
            set
            {
                _default = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        
        static MemberBindingBehaviour()
        {
            _default = BindNoMembersBehaviour.Instance;
        }
    }
}
