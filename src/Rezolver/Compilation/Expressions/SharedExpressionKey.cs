// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Key for a shared expression used during expression tree generation.  As a consumer of this library, you
	/// are unlikely ever to need to use it.
	/// </summary>
	public class SharedExpressionKey : IEquatable<SharedExpressionKey>
	{
		/// <summary>
		/// Gets the type that registered the shared expression
		/// </summary>
		public Type RequestingType { get; private set; }
		/// <summary>
		/// The intended type of the expression that is cached by this key.
		/// </summary>
		public Type TargetType { get; private set; }
		/// <summary>
		/// Gets the name used for expressions that are cached using this key.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SharedExpressionKey"/> class.
		/// </summary>
		/// <param name="targetType">Required. Eventual runtime type of the object produced by the expression that will be cached using this key.</param>
		/// <param name="name">Required. The name used for storing and retrieving expressions cached with this key.</param>
		/// <param name="requestingType">The type (e.g. the runtime type of an <see cref="ITarget"/> implementation) whose compilation requires the cached expression.</param>
		public SharedExpressionKey(Type targetType, string name, Type requestingType = null)
		{
			targetType.MustNotBeNull("targetType");
			name.MustNotBeNull("name");
			TargetType = targetType;
			Name = name;
			RequestingType = requestingType;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return base.Equals(obj as SharedExpressionKey);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return TargetType.GetHashCode() ^
			  Name.GetHashCode() ^
			  (RequestingType != null ? RequestingType.GetHashCode() : 0);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		public bool Equals(SharedExpressionKey other)
		{
			return object.ReferenceEquals(this, other) ||
			  (RequestingType == other.RequestingType && TargetType == other.TargetType && Name == other.Name);
		}
	}
}
