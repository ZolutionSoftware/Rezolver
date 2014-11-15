using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// abstract base class to serve as a starting point for implementing the ITypeReference interface.
	/// </summary>
	public abstract class TypeReferenceBase : ITypeReference
	{
		public abstract string TypeName
		{
			get;
		}

		public abstract ITypeReference[] GenericArguments
		{
			get;
		}
	}
}
