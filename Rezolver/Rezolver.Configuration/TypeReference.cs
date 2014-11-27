using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class TypeReference : TypeReferenceBase
	{
		public static readonly ITypeReference[] NoGenericArguments = new ITypeReference[0];
		
		private static readonly ITypeReference _openGenericTypeArgument = new TypeReference("#TArg#", null);

		/// <summary>
		/// The one-and-only open generic argument instance.
		/// 
		/// The only way to fetch a non-derived TypeReference that returns true for <see cref="IsOpenGenericTypeArgument"/>
		/// is to use the reference from this field.
		/// </summary>
		public static ITypeReference OpenGenericTypeArgument
		{
			get
			{
				return _openGenericTypeArgument;
			}
		}


		public override bool IsOpenGenericTypeArgument
		{
			get
			{
				return object.ReferenceEquals(OpenGenericTypeArgument, this);
			}
		}

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="lineInfo">Optional.</param>
		/// <param name="genericArguments"></param>
		public TypeReference(string typeName, IConfigurationLineInfo lineInfo, params ITypeReference[] genericArguments)
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
