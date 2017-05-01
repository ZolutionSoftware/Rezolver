using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// This is the default <see cref="IMemberBindingBehaviour"/> which doesn't bind any members.
    /// </summary>
    public class BindNoMembersBehaviour : IMemberBindingBehaviour
    {
        private static MemberBinding[] NoBindings = new MemberBinding[0];

        /// <summary>
        /// The one and only instance of the <see cref="BindNoMembersBehaviour"/>
        /// </summary>
        public static BindNoMembersBehaviour Instance { get; } = new BindNoMembersBehaviour();
        private BindNoMembersBehaviour() { }

        /// <summary>
        /// Implementation of <see cref="IMemberBindingBehaviour.GetMemberBindings(ICompileContext, Type)"/>
        /// 
        /// Always returns an empty array.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MemberBinding[] GetMemberBindings(ICompileContext context, Type type)
        {
            return NoBindings;
        }
    }
}
