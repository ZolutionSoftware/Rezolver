using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Concrete implementation of the <see cref="TypeReferenceBase" /> abstract class, and the default implementation
	/// of <see cref="ITypeReference" /> to use when parsing configuration.
	/// </summary>
	public class TypeReference : TypeReferenceBase
	{
		public static readonly ITypeReference[] NoGenericArguments = new ITypeReference[0];
		
		private static readonly ITypeReference _openGenericTypeArgument = new TypeReference("#TArg#", null);

		/// <summary>
		/// The one-and-only open generic argument instance.
		/// The only way to fetch a non-derived TypeReference that returns true for <see cref="IsOpenGenericTypeArgument" />
		/// is to use the reference from this field.
		/// </summary>
		/// <value>The open generic type argument.</value>
		public static ITypeReference OpenGenericTypeArgument
		{
			get
			{
				return _openGenericTypeArgument;
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance represents an open generic type argument (e.g. the 'T' from List&lt;T&gt;).
		/// </summary>
		/// <value><c>true</c> if this instance is an open generic type argument; otherwise, <c>false</c>.</value>
		public override bool IsOpenGenericTypeArgument
		{
			get
			{
				return object.ReferenceEquals(OpenGenericTypeArgument, this);
			}
		}

		private readonly string _typeName;
		/// <summary>
		/// Gets the name of the type.
		/// </summary>
		/// <value>The name of the type.</value>
		public override string TypeName
		{
			get { return _typeName; }
		}

		private readonly ITypeReference[] _genericArguments;
		public override ITypeReference[] GenericArguments
		{
			get { return _genericArguments ?? NoGenericArguments; }
		}

		private readonly bool _isArray;

		public override bool IsArray
		{
			get { return _isArray; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeReference"/> class.
		/// </summary>
		/// <param name="typeName">Name of the type.</param>
		/// <param name="lineInfo">Optional.</param>
		/// <param name="genericArguments">The generic arguments.</param>
		public TypeReference(string typeName, IConfigurationLineInfo lineInfo, params ITypeReference[] genericArguments)
			: this(typeName, lineInfo, false, genericArguments)
		{
			
		}

		public TypeReference(string typeName, IConfigurationLineInfo lineInfo, bool isArray, params ITypeReference[] genericArguments)
			: base(lineInfo)
		{
			if (string.IsNullOrWhiteSpace(typeName))
				throw new ArgumentException("string cannot be null, empty or all white space", "typeName");
			if (genericArguments != null && genericArguments.Any(t => t == null))
				throw new ArgumentException("all type references passed as generic arguments must be non-null", "genericArguments");

			_typeName = typeName;
			_genericArguments = genericArguments;
			_isArray = isArray;
		}
	}
}
