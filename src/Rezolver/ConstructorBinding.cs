// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Represents a binding specifically to a class constructor, optionally with an additional
    /// set of <see cref="MemberBindings"/> to be used to initialise a new instance's properties
    /// or fields directly.
    /// </summary>
    /// <seealso cref="Rezolver.MethodBinding" />
    public class ConstructorBinding : MethodBinding
    {
        /// <summary>
        /// An empty array of <see cref="MemberBinding"/> objects used to represent a
        /// constructor binding with no bound members.  The <see cref="MemberBindings"/> property
        /// will be set to this if the constructor is called with a null <c>memberBindings</c> argument.
        /// </summary>
        public static MemberBinding[] NoBoundMembers = new MemberBinding[0];
        /// <summary>
        /// Gets the constructor to be invoked. Note that this simply returns the
        /// base <see cref="MethodBinding.Method"/> property cast to <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <value>The constructor.</value>
        public ConstructorInfo Constructor { get { return (ConstructorInfo)Method; } }

        /// <summary>
        /// Gets the member bindings to be applied to the new instance created by the <see cref="Constructor"/>
        /// </summary>
        /// <value>The member bindings.</value>
        /// <remarks>Member bindings represent the inline initialisation of writable properties or fields
        /// immediately after constructing a new instance of a type.</remarks>
        public MemberBinding[] MemberBindings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorBinding" /> class.
        /// </summary>
        /// <param name="constructor">The constructor to be used .</param>
        /// <param name="boundArgs">Optional.  The bound arguments.  Can be null or empty.</param>
        /// <param name="memberBindings">Optional.  The bindings for the members of the new instance created by the constructor.</param>
        public ConstructorBinding(ConstructorInfo constructor,
            ParameterBinding[] boundArgs = null,
            MemberBinding[] memberBindings = null)
            : base(constructor, boundArgs)
        {
            MemberBindings = memberBindings ?? NoBoundMembers;
        }
    }
}
