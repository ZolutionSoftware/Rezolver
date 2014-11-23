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

		public abstract bool IsOpenGenericTypeArgument
		{
			get;
		}

		/// <summary>
		/// Produces a string representation of the type name, including generic arguments.
		/// 
		/// Please note - this is not intended to produce a typename that can be fed to System.Type.GetType, even if 
		/// occasionally it does.
		/// </summary>
		public override string ToString()
		{
			if (GenericArguments == null || GenericArguments.Length == 0)
				return FormatTypeName(TypeName);

			return string.Format("{0}[{1}]", FormatTypeName(TypeName), string.Join(", ", GenericArguments.Select(t => t ?? new TypeReference("null"))));
		}

		private string FormatTypeName(string typeName)
		{
			if (typeName.Contains(","))
				return string.Concat("[", typeName, "]");
			else
				return typeName;
		}
	}
}
