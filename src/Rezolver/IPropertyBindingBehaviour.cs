using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Describes a type which discovers auto-binds properties to a rezolve context for 
	/// the purposes of building a ConstructorTarget
	/// </summary>
	public interface IPropertyBindingBehaviour
	{
		/// <summary>
		/// Retrieves the property and/or field bindings for the given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		PropertyOrFieldBinding[] GetPropertyBindings(Type type);
	}

}
