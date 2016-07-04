// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
		/// <summary>
		/// Gets the line number within the configuration source that contains the start of the text from which this object was parsed.
		/// Used in conjunction with <see cref="StartLinePos" />, it allows you to zero-in on the exact starting point of this parsed object.
		/// </summary>
		/// <value>The start line number.</value>
		public int? StartLineNo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the position from the start of the line, indicated by <see cref="StartLineNo" />, where the configuration text
		/// begins for this parsed object.
		/// </summary>
		/// <value>The start line position.</value>
		public int? StartLinePos
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the line number within the configuration source that sees the end of the text from which this object was parsed.
		/// Used in conjunction with <see cref="EndLinePos" />, it allows you to zero-in on the exact ending of this parsed object.
		/// </summary>
		/// <value>The end line number.</value>
		public int? EndLineNo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the position from the start of the line, indicated by <see cref="EndLineNo" />, where the configuration text
		/// ends for this parsed object.
		/// </summary>
		/// <value>The end line position.</value>
		public int? EndLinePos
		{
			get;
			private set;
		}

		/// <summary>
		/// The root type name.
		/// </summary>
		/// <value>The name of the type.</value>
		public abstract string TypeName
		{
			get;
		}

		/// <summary>
		/// Any explicitly provided generic arguments are stored here.
		/// Note that it might turn out that the TypeName refers to a whole closed generic type, in which
		/// case the referenced type could still be generic even if this array is empty.
		/// It's also the case that arguments could be passed here when the root type name resolves to
		/// a non-generic type definition, in which case type resolution will likely fail.
		/// </summary>
		/// <value>The generic arguments.</value>
		public abstract ITypeReference[] GenericArguments
		{
			get;
		}

		/// <summary>
		/// True if this type represents an open generic argument - this is how to explicitly reference an open generic type in a type reference:
		/// you specify a base type, then have one or more open generic arguments specified in the GenericArguments array.  If all are
		/// open generic arguments, then you have created a reference to the fully open generic type.
		/// You only need to provide all-open arguments if the base <see cref="TypeName" /> could be ambiguous between a non generic and generic type,
		/// or there are multiple generic types with the same base name.
		/// Equally, you can do this to create references to partially open generics, which may or may not be supported by the adapter or
		/// the target that is built.
		/// </summary>
		/// <value><c>true</c> if this instance is open generic type argument; otherwise, <c>false</c>.</value>
		public abstract bool IsOpenGenericTypeArgument
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the type ultimately represented by this instance is an array of the
		/// type described by the rest of this instance's properties.
		/// </summary>
		/// <value><c>true</c> if this instance represents an array type; otherwise, <c>false</c>.</value>
		public abstract bool IsArray
		{ 
			get; 
		}

		/// <summary>
		/// Gets a value indicating whether this instance represents a type that is to be late-bound for a specific target type.
		/// </summary>
		/// <value><c>true</c> if this instance is unbound; otherwise, <c>false</c>.</value>
		public abstract bool IsUnbound
		{
			get;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeReferenceBase"/> class.
		/// </summary>
		protected TypeReferenceBase() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeReferenceBase"/> class, copying the passed
		/// line information into this object, if provided.
		/// </summary>
		/// <param name="lineInfo">Optional.  The line information.</param>
		protected TypeReferenceBase(IConfigurationLineInfo lineInfo)
		{
			if (lineInfo != null)
			{
				StartLineNo = lineInfo.StartLineNo;
				StartLinePos = lineInfo.StartLinePos;
				EndLineNo = lineInfo.EndLineNo;
				EndLinePos = lineInfo.EndLinePos;
			}
		}

		/// <summary>
		/// Produces a string representation of the type name, including generic arguments.
		/// 
		/// Please note - this is not intended to produce a type name that can be fed to System.Type.GetType, even if 
		/// occasionally it does.
		/// </summary>
		public override string ToString()
		{
			if (GenericArguments == null || GenericArguments.Length == 0)
				return FormatTypeName(TypeName);

			return string.Format("{0}[{1}]{2}", FormatTypeName(TypeName), string.Join(", ", GenericArguments.Select(t => t ?? new TypeReference("null", null, false))), IsArray ? "[]" : "");
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
