// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Implements <see cref="ITarget"/> by wrapping a single instance that's already been constructed.
	/// 
	/// By default, scope tracking is disabled (since the caller owns the object, not the Rezolver framework)).  If it's
	/// enabled, then scope tracking behaves exactly the same as <see cref="SingletonTarget"/>.
	/// </summary>
	public class ObjectTarget : TargetBase, ICompiledTarget
	{

		private readonly Type _declaredType;
		private readonly bool _suppressScopeTracking;

		protected override bool SuppressScopeTracking
		{
			get
			{
				return _suppressScopeTracking;
			}
		}

		/// <summary>
		/// Gets the value that will be exposed by expressions built by this instance.
		/// </summary>
		/// <value>The value.</value>
		public object Value { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="ObjectTarget"/> class.
		/// </summary>
		/// <param name="obj">The object to be returned by this target when resolved.</param>
		/// <param name="declaredType">Optional.  The declared type of this target, if different from the absolute type of the <paramref name="obj"/></param>
		/// <param name="suppressScopeTracking">Optional.  Controls whether the instance will be added to a scope when resolved.  If true (the default) then
		/// no scope tracking is performed, and you will have to dispose of the object, if disposable.  If false, then the object will be tracked by the 
		/// ROOT scope of the first scope the object is resolved from.</param>
		/// <remarks>Please note - if you enable scope tracking, but the object is never resolved, then the object will not be disposed and you will need
		/// to ensure you dispose of it.</remarks>
		public ObjectTarget(object obj, Type declaredType = null, bool suppressScopeTracking = true)
		{
			Value = obj;
			_suppressScopeTracking = suppressScopeTracking;
			//if the caller provides a declared type we check
			//also that, if the object is null, the target type
			//can accept nulls.  Otherwise we're simply checking 
			//that the value that's supplied is compatible with the 
			//type that is being declared.
			if (declaredType != null)
			{
				if (Value == null)
				{
					if (!declaredType.CanBeNull())
						throw new ArgumentException(string.Format(ExceptionResources.TargetIsNullButTypeIsNotNullable_Format, declaredType), "declaredType");
				}
				else if (!TypeHelpers.AreCompatible(Value.GetType(), declaredType))
					throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, Value.GetType()), "declaredType");

				_declaredType = declaredType;
			}
			else //an untyped null is typed as Object
				_declaredType = Value == null ? typeof(object) : Value.GetType();
		}

		protected override Expression CreateScopeSelectionExpression(ICompileContext context, Expression expression)
		{
			//when scope tracking is enabled (not the default), then we behave like a singleton - and track in the root scope.
			//return ExpressionHelper.Make_Scope_GetScopeRootCallExpression(context);
			throw new NotImplementedException();
		}

		object ICompiledTarget.GetObject(ResolveContext context)
		{
			//when directly implementing ICompiledTarget, the scoping rules have to be honoured manually
			if (SuppressScopeTracking || context.Scope == null)
				return Value;
			else {
				return context.Scope.GetScopeRoot().GetOrAdd(context, c => Value, false);
			}
		}

		/// <summary>
		/// Gets the declared type of object that is constructed by this target.  This will be the return type of
		/// any expression built by <see cref="CreateExpression" /> unless otherwise instructed to build a different type.
		/// </summary>
		/// <value>The type of the declared.</value>
		public override Type DeclaredType
		{
			get
			{
				return _declaredType;
			}
		}
	}

	/// <summary>
	/// Extension method(s)
	/// </summary>
	public static class ObjectTargetExtensions
	{
		/// <summary>
		/// Wraps the instance on which this is invoked as an <see cref="ObjectTarget"/> that can be registered into an <see cref="ITargetContainer"/>.
		/// 
		/// The parameters are direct analogues of the parameters on the type's constructor (see <see cref="ObjectTarget.ObjectTarget(object, Type, bool)"/>).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="declaredType"></param>
		/// <param name="suppressScopeTracking"></param>
		/// <returns>A new object target that wraps the object <paramref name="obj"/>.</returns>
		public static ObjectTarget AsObjectTarget<T>(this T obj, Type declaredType = null, bool suppressScopeTracking = true)
		{
			return new ObjectTarget(obj, declaredType ?? typeof(T), suppressScopeTracking: suppressScopeTracking);
		}
	}
}
