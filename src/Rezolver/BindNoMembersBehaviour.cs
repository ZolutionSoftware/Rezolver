// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// This is the default <see cref="IMemberBindingBehaviour"/> which doesn't bind any members.  It's
    /// a singleton accessible only via the <see cref="MemberBindingBehaviour.BindNone"/> static property.
    /// </summary>
    public sealed class BindNoMembersBehaviour : IMemberBindingBehaviour
    {
        private static readonly MemberBinding[] NoBindings = new MemberBinding[0];

        internal BindNoMembersBehaviour() { }

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
