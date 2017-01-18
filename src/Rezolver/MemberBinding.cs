// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
				PropertyInfo p = Member as PropertyInfo;
				if (p != null)
					return p.PropertyType;
				else
					return ((FieldInfo)Member).FieldType;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberBinding"/> class.
		/// </summary>
		/// <param name="member">The member to be bound.</param>
		/// <param name="target">The target whose value will be written to the member.</param>
		public MemberBinding(MemberInfo member, ITarget target)
		{
			member.MustNotBeNull(nameof(member));
			target.MustNotBeNull(nameof(target));
			Member = member;
			Target = target;
		}
	}
}
