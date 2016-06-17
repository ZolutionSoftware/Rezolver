using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Describes a type which discovers property/field bindings
	/// </summary>
	public interface IPropertyBindingBehaviour
	{
		/// <summary>
		/// Retrieves the property and/or field bindings for the given type based on the given <see cref="CompileContext"/>
		/// </summary>
		/// <param name="context"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		PropertyOrFieldBinding[] GetPropertyBindings(CompileContext context, Type type);
	}

}
