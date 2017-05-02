// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Targets;

namespace Rezolver
{
	/// <summary>
	/// This implementation of <see cref="IMemberBindingBehaviour"/> binds all publicly writable
	/// properties and public fields to values obtained from the container.
	/// </summary>
	/// <seealso cref="Rezolver.IMemberBindingBehaviour" />
	/// <remarks>This is a stateless singleton accessible through the <see cref="MemberBindingBehaviour.BindAll"/> 
    /// static property.
	/// 
	/// The class also serves as a good starting point for any custom binding behaviours you might need - as there
	/// are numerous virtual methods which allow you to change which fields and/or properties are selected for binding,
	/// as well as how those bindings are created.
	/// 
	/// The default behaviour is to bind each member to a new <see cref="ResolvedTarget"/> whose 
	/// <see cref="ResolvedTarget.DeclaredType"/> is set to the member's type.</remarks>
	public class BindAllMembersBehaviour : IMemberBindingBehaviour
	{
		private static readonly Lazy<BindAllMembersBehaviour> _instance = new Lazy<BindAllMembersBehaviour>(() => new BindAllMembersBehaviour());
		/// <summary>
		/// Gets the one and only instance of <see cref="BindAllMembersBehaviour"/>
		/// </summary>
		internal static BindAllMembersBehaviour Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		/// <summary>
		/// Constructs a new instance of the <see cref="BindAllMembersBehaviour"/> class.
		/// </summary>
		protected BindAllMembersBehaviour() { }

		/// <summary>
		/// Implementation of <see cref="IMemberBindingBehaviour.GetMemberBindings(ICompileContext, Type)"/>.
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <remarks>The base implementation calls <see cref="GetBindableProperties(ICompileContext, Type)"/>, passing the resultant enumerable to
		/// the <see cref="BindProperties(ICompileContext, Type, IEnumerable{PropertyInfo})"/> function; it also does the same thing with
		/// <see cref="GetBindableFields(ICompileContext, Type)"/> and <see cref="BindFields(ICompileContext, Type, IEnumerable{FieldInfo})"/> - 
		/// concatenating the two enumerables together and returning the result as an array of <see cref="MemberBinding"/> objects.</remarks>
		public virtual MemberBinding[] GetMemberBindings(ICompileContext context, Type type)
		{
			//find all publicly writable properties and public fields, emit 
			return BindProperties(context, type, GetBindableProperties(context, type))
			  .Concat(BindFields(context, type, GetBindableFields(context, type))).ToArray();
		}

		/// <summary>
		/// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/> - iterates through the 
		/// <paramref name="fields"/>, calling <see cref="CreateBinding(ICompileContext, Type, FieldInfo)"/> for each,
		/// and those which are non-null.
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <param name="fields">The fields for which bindings are to be created.  This is fed by
		/// the function <see cref="GetBindableFields(ICompileContext, Type)"/></param>
		/// <returns></returns>
		protected virtual IEnumerable<MemberBinding> BindFields(ICompileContext context, Type type, IEnumerable<FieldInfo> fields)
		{
			return fields.Select(f => CreateBinding(context, type, f)).Where(b => b != null);
		}

		/// <summary>
		/// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/> - iterates through the 
		/// <paramref name="properties"/>, calling <see cref="CreateBinding(ICompileContext, Type, PropertyInfo)"/> for each,
		/// and those which are non-null.
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <param name="properties">The properties for which bindings are to be created.  This is fed by
		/// the function <see cref="GetBindableProperties(ICompileContext, Type)"/></param>
		/// <returns>An enumerable of <see cref="MemberBinding"/> objects representing the bindings to be used
		/// for each bindable property in <paramref name="properties"/>.</returns>
		protected virtual IEnumerable<MemberBinding> BindProperties(ICompileContext context, Type type, IEnumerable<PropertyInfo> properties)
		{
			return properties.Select(p => CreateBinding(context, type, p)).Where(b => b != null);
		}

		/// <summary>
		/// Creates a binding for the given field.
		/// 
		/// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/>
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <param name="field">The field for which a binding is to be created.</param>
		/// <remarks>Override this method to customise the binding that is create for the given
		/// field, or to prevent the binding from being created at all(return <c>null</c> if you want
		/// to abort binding the field).
		/// 
		/// The base implementation simply creates a new <see cref="MemberBinding"/> whose
		/// <see cref="MemberBinding.Target"/> is set to a new <see cref="ResolvedTarget"/> for the type
		/// <see cref="FieldInfo.FieldType"/> - thus causing the field to be assigned a value
		/// resolved from the container when the instance is created.</remarks>
		protected virtual MemberBinding CreateBinding(ICompileContext context, Type type, FieldInfo field)
		{
			return new MemberBinding(field, new ResolvedTarget(field.FieldType));
		}

		/// <summary>
		/// Creates a binding for the given property.
		/// 
		/// Called by <see cref="GetMemberBindings(ICompileContext, Type)"/>
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <param name="prop">The property for which a binding is to be created.</param>
		/// <remarks>Override this method to customise the binding that is create for the given
		/// property, or to prevent the binding from being created at all(return <c>null</c> if you want
		/// to abort binding the property).
		/// 
		/// The base implementation simply creates a new <see cref="MemberBinding"/> whose
		/// <see cref="MemberBinding.Target"/> is set to a new <see cref="ResolvedTarget"/> for the type
		/// <see cref="PropertyInfo.PropertyType"/> - thus causing the property to be assigned a value
		/// resolved from the container when the instance is created.</remarks>
		protected virtual MemberBinding CreateBinding(ICompileContext context, Type type, PropertyInfo prop)
		{
			return new MemberBinding(prop, new ResolvedTarget(prop.PropertyType));
		}

		/// <summary>
		/// Gets the bindable fields on the <paramref name="type"/>.
		/// 
		/// Used by <see cref="GetMemberBindings(ICompileContext, Type)"/> and passed to the
		/// <see cref="BindFields(ICompileContext, Type, IEnumerable{FieldInfo})"/> method.
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <remarks>Override this method to filter the fields which can be bound.  The base implementation
		/// returns all public instance fields declared on the <paramref name="type"/>.
		/// </remarks>
		protected virtual IEnumerable<FieldInfo> GetBindableFields(ICompileContext context, Type type)
		{
			return type.GetInstanceFields().Public();
		}

		/// <summary>
		/// Gets the bindable properties on the <paramref name="type"/>.
		/// 
		/// Used by <see cref="GetMemberBindings(ICompileContext, Type)"/> and passed to the 
		/// <see cref="BindProperties(ICompileContext, Type, IEnumerable{PropertyInfo})"/> method.
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <param name="type">The type whose members are to be bound.</param>
		/// <remarks>Override this method to filter the properties which can be bound.  The base implementation
		/// returns all instance properties which have publicly accessible 'set' accessors.</remarks>
		protected virtual IEnumerable<PropertyInfo> GetBindableProperties(ICompileContext context, Type type)
		{
			return type.GetInstanceProperties().PubliclyWritable().Where(pi => pi.GetIndexParameters()?.Length == 0);
		}
	}
}