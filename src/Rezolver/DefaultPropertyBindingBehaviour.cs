﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// The default implementation of <see cref="IPropertyBindingBehaviour"/> when you are creating a
	/// <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> and you want publicly writable
	/// properties and public fields to be assigned values obtained from the container.
	/// 
	/// If you do not require properties or fields to be bound from the container, then use a null <see cref="IPropertyBindingBehaviour"/>.
	/// </summary>
	/// <seealso cref="Rezolver.IPropertyBindingBehaviour" />
	/// <remarks>This is a singleton class accessible through the <see cref="Instance"/> static property.</remarks>
	public class DefaultPropertyBindingBehaviour : IPropertyBindingBehaviour
	{
		private static readonly Lazy<DefaultPropertyBindingBehaviour> _instance = new Lazy<DefaultPropertyBindingBehaviour>(() => new DefaultPropertyBindingBehaviour());
		/// <summary>
		/// Gets the one and only instance of <see cref="DefaultPropertyBindingBehaviour"/>
		/// </summary>
		public static DefaultPropertyBindingBehaviour Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		/// <summary>
		/// Implementation of <see cref="IPropertyBindingBehaviour.GetPropertyBindings(CompileContext, Type)"/>.
		/// 
		/// Returns a binding for each publicly writable property (i.e. with a public set accessor) and each public
		/// field on the <paramref name="type"/>.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="type">The type.</param>
		public virtual PropertyOrFieldBinding[] GetPropertyBindings(CompileContext context, Type type)
		{
			//find all publicly writable properties and public fields, emit 
			return BindProperties(type, GetBindableProperties(type))
			  .Concat(BindFields(type, GetBindableFields(type))).ToArray();
		}

		private IEnumerable<PropertyOrFieldBinding> BindFields(Type type, IEnumerable<FieldInfo> fields)
		{
			foreach (var field in fields)
			{
				yield return CreateBinding(type, field);
			}
		}

		protected virtual IEnumerable<PropertyOrFieldBinding> BindProperties(Type type, IEnumerable<PropertyInfo> properties)
		{
			foreach (var prop in properties)
			{
				yield return CreateBinding(type, prop);
			}
		}

		/// <summary>
		/// Creates the binding for the given field.
		/// 
		/// Called by <see cref="GetPropertyBindings(CompileContext, Type)"/>
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="field">The field.</param>
		protected virtual PropertyOrFieldBinding CreateBinding(Type type, FieldInfo field)
		{
			return new PropertyOrFieldBinding(field, new RezolvedTarget(field.FieldType));
		}

		/// <summary>
		/// Creates the binding for the given property.
		/// 
		/// Called by <see cref="GetPropertyBindings(CompileContext, Type)"/>
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="prop">The property.</param>
		protected virtual PropertyOrFieldBinding CreateBinding(Type type, PropertyInfo prop)
		{
			return new PropertyOrFieldBinding(prop, new RezolvedTarget(prop.PropertyType));
		}

		/// <summary>
		/// Gets the bindable fields on the <paramref name="type"/>.
		/// 
		/// Used by <see cref="GetPropertyBindings(CompileContext, Type)"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		protected virtual IEnumerable<FieldInfo> GetBindableFields(Type type)
		{
			return type.GetInstanceFields().Public();
		}

		/// <summary>
		/// Gets the bindable properties on the <paramref name="type"/>.
		/// 
		/// Used by <see cref="GetPropertyBindings(CompileContext, Type)"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		protected virtual IEnumerable<PropertyInfo> GetBindableProperties(Type type)
		{
			//slightly 
			return type.GetInstanceProperties().PubliclyWritable();
		}
	}
}