using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	public class DefaultPropertyBindingBehaviour : IPropertyBindingBehaviour
	{
		private static readonly Lazy<DefaultPropertyBindingBehaviour> _instance = new Lazy<DefaultPropertyBindingBehaviour>(() => new DefaultPropertyBindingBehaviour());
		public static DefaultPropertyBindingBehaviour Instance
		{
			get
			{
				return _instance.Value;
			}
		}

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

		protected virtual PropertyOrFieldBinding CreateBinding(Type type, FieldInfo field)
		{
			return new PropertyOrFieldBinding(field, new RezolvedTarget(field.FieldType));
		}

		protected virtual PropertyOrFieldBinding CreateBinding(Type type, PropertyInfo prop)
		{
			return new PropertyOrFieldBinding(prop, new RezolvedTarget(prop.PropertyType));
		}

		protected virtual IEnumerable<FieldInfo> GetBindableFields(Type type)
		{
			return type.GetInstanceFields().Public();
		}

		protected virtual IEnumerable<PropertyInfo> GetBindableProperties(Type type)
		{
			//slightly 
			return type.GetInstanceProperties().PubliclyWritable();
		}
	}
}