using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class TypeReference : TypeReferenceBase
	{
		public static readonly ITypeReference[] NoGenericArguments = new ITypeReference[0];

		private readonly string _typeName;
		public override string TypeName
		{
			get { return _typeName; }
		}

		private readonly ITypeReference[] _genericArguments;
		public override ITypeReference[] GenericArguments
		{
			get { return _genericArguments ?? NoGenericArguments; }
		}

		public TypeReference(string typeName, params ITypeReference[] genericArguments)
		{
			if (string.IsNullOrWhiteSpace(typeName))
				throw new ArgumentException("string cannot be null, empty or all whitespace", "typeName");
			if (genericArguments.Any(t => t == null))
				throw new ArgumentException("all type references passed as generic arguments must be non-null", "genericArguments");

			_typeName = typeName;
			_genericArguments = genericArguments;
		}
	}
}
