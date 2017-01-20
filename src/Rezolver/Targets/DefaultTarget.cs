// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Targets
{
	/// <summary>
	/// A target that simply creates a default instance of a given type.  I.e. the same
	/// as doing default(type) in C#.
	/// </summary>
	/// <remarks>The type also implements the <see cref="ICompiledTarget"/> interface for direct
	/// resolving.</remarks>
	public class DefaultTarget : TargetBase, ICompiledTarget
	{
		private static class Default<T>
		{
			public static readonly T Value = default(T);
		}
		private static readonly ConcurrentDictionary<Type, Func<object>> _defaultCallbacks = new ConcurrentDictionary<Type, Func<object>>();

		private static object GetDefault(Type type)
		{
			return _defaultCallbacks.GetOrAdd(type, t => {
				var tDefault = typeof(Default<>).MakeGenericType(type);
				return Expression.Lambda<Func<object>>(
					//the convert is important to handle boxing conversions for value types.
					Expression.Convert(
						Expression.Field(null, tDefault.GetStaticFields().Single(f => f.Name == "Value")), typeof(object)
					)
				).Compile();
			});
		}

		object ICompiledTarget.GetObject(ResolveContext context)
		{
			return GetDefault(context.RequestedType);
		}

		private readonly Type _declaredType;

		/// <summary>
		/// Always equal to the type for which the default value will be returned
		/// </summary>
		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultTarget"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		public DefaultTarget(Type type)
		{
			type.MustNotBeNull("type");
			_declaredType = type;
		}
	}
}