// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Represents the binding of an <see cref="ITarget"/> to a property or field of a given type.
    ///
    /// Not to be confused with the type of the same name from the System.Linq.Expressions namespace, although
    /// they are technically equivalent.
    /// </summary>
    /// <remarks>You typically don't create this type directly - instead, other targets such as <see cref="ConstructorTarget"/>
    /// will create it as needed through the use of an <see cref="IMemberBindingBehaviour"/> object.
    ///
    /// However, the two constructors allow you either to create a binding to a specific target, or to
    /// create a binding that will automatically resolve the member type from the container.
    /// </remarks>
    public class MemberBinding
    {
        /// <summary>
        /// Empty bindings.
        /// </summary>
        public static readonly MemberBinding[] None = new MemberBinding[0];

        /// <summary>
        /// Gets the member against which this binding is to be applied.
        /// </summary>
        /// <value>The member.</value>
        public MemberInfo Member { get; private set; }

        /// <summary>
        /// Gets the target whose value when resolved will be written to the <see cref="Member"/>
        /// </summary>
        /// <value>The target.</value>
        public ITarget Target { get; private set; }

        /// <summary>
        /// Gets the type of the <see cref="Member"/>.  E.g. if the member represents a String property
        /// on the declaring type, then this will return the <see cref="System.String"/> type.
        ///
        /// If the member represents an integer field, this it will return the <see cref="System.Int32"/> type.
        /// </summary>
        /// <value>The type of the member.</value>
        public Type MemberType
        {
            get
            {
                if (Member is PropertyInfo p)
                {
                    return p.PropertyType;
                }
                else
                {
                    return ((FieldInfo)Member).FieldType;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MemberBinding"/> class which will seek to bind
        /// the given <paramref name="member"/> with the result of the given <paramref name="target"/>.
        /// </summary>
        /// <param name="member">The member to be bound.</param>
        /// <param name="target">The target whose value will be written to the member.</param>
        public MemberBinding(MemberInfo member, ITarget target)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MemberBinding"/> class which will auto-bind
        /// the <paramref name="member"/> by resolving an instance of the member's type, or
        /// to the type passed in the <paramref name="resolveType"/> parameter.
        /// </summary>
        /// <param name="member">The member to be bound.</param>
        /// <param name="resolveType">If not <c>null</c> then this will be the type that'll
        /// be resolved from the container.  Otherwise, the member's type will be resolved from the
        /// container.</param>
        public MemberBinding(MemberInfo member, Type resolveType = null)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Target = Rezolver.Target.Resolved(resolveType ?? MemberType);
        }
    }
}
