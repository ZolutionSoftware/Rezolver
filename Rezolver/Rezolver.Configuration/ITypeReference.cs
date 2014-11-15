using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Captures a reference to a type made in a configuration file.  It does not guarantee that the type
	/// can be located, it simply provides a common interface for storing the type information written
	/// in a configuration file.
	/// 
	/// An IConfigurationAdapter instance will need to resolve the actual runtime type from this when registering
	/// targets from a configuration file.
	/// </summary>
	public interface ITypeReference
	{
		/// <summary>
		/// The root type name.
		/// </summary>
		string TypeName { get; }
		/// <summary>
		/// Any explicitly provided generic arguments are stored here.
		/// 
		/// Note that it might turn out that the TypeName refers to a whole closed generic type, in which
		/// case the referenced type could still be generic even if this array is empty.
		/// 
		/// It's also the case that arguments could be passed here when the root type name resolves to
		/// a non-generic type definition, in which case type resolution will likely fail.
		/// </summary>
		ITypeReference[] GenericArguments { get; }
	}
}
