// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// Implements <see cref="ITarget"/> using either a nullary factory of the type <see cref="Func{TResult}"/>
	/// or a unary factory of the type <see cref="Func{T, TResult}"/> in which the argument is <see cref="RezolveContext"/>.
	/// </summary>
	/// <typeparam name="T">The type of object produced by the delegate passed on construction</typeparam>
	public class DelegateTarget<T> : TargetBase
	{
		private readonly Type _declaredType;

		/// <summary>
		/// Gets the nullary factory (one which doesn't take any arguments) to be executed by the compiled code produced
		/// by this target.  If this is <c>null</c> then <see cref="UnaryFactory"/> will not be.
		/// </summary>
		public Func<T> NullaryFactory { get; }

		/// <summary>
		/// Gets the unary factory (one which accepts one argument of type <see cref="RezolveContext"/>) to be executed
		/// by the compiled code produced by this target.  If this is <c>null</c> then <see cref="NullaryFactory"/> will not be.
		/// </summary>
		public Func<RezolveContext, T> UnaryFactory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T}"/> class which will use
		/// a nullary factory to produce an instance.
		/// </summary>
		/// <param name="nullaryFactory">Required. The nullary factory.</param>
		/// <param name="declaredType">Optional. Static type that will be set to this target's <see cref="DeclaredType"/>
		/// if different from <typeparamref name="T"/>.  If provided it must be a base or interface of <typeparamref name="T"/>.</param>
		/// <exception cref="ArgumentException">If <typeparamref name="T"/> is not compatible with <paramref name="declaredType" /></exception>
		/// <exception cref="ArgumentNullException">If <paramref name="nullaryFactory"/> is null.</exception> 
		public DelegateTarget(Func<T> nullaryFactory, Type declaredType = null)
		{
			nullaryFactory.MustNotBeNull(nameof(nullaryFactory));
			NullaryFactory = nullaryFactory;
			if (declaredType != null)
			{
				if (!TypeHelpers.AreCompatible(typeof(T), declaredType) && !TypeHelpers.AreCompatible(declaredType, typeof(T)))
					throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, typeof(T)));
			}
			_declaredType = declaredType ?? typeof(T);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget{T}"/> class which will use
		/// a unary factory to produce an instance.
		/// </summary>
		/// <param name="unaryFactory">Required. The unary factory, which accepts a single argument of type <see cref="RezolveContext"/>.</param>
		/// <param name="declaredType">Optional. Static type that will be set to this target's <see cref="DeclaredType"/>
		/// if different from <typeparamref name="T"/>.  If provided it must be a base or interface of <typeparamref name="T"/>.</param>
		/// <exception cref="ArgumentException">If <typeparamref name="T"/> is not compatible with <paramref name="declaredType" /></exception>
		/// <exception cref="ArgumentNullException">If <paramref name="unaryFactory"/> is null.</exception> 
		public DelegateTarget(Func<RezolveContext, T> unaryFactory, Type declaredType = null)
		{
			unaryFactory.MustNotBeNull("factory");
			UnaryFactory = unaryFactory;
			if (declaredType != null)
			{
				if (!TypeHelpers.AreCompatible(typeof(T), declaredType) && !TypeHelpers.AreCompatible(declaredType, typeof(T)))
					throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, typeof(T)));
			}
			_declaredType = declaredType ?? typeof(T);
		}

		/// <summary>
		/// Returns an expression that represents invoking whichever factory was passed to this instance on construction.
		/// 
		/// When compiled and executed, the factory will be called to produce an instance of <typeparamref name="T"/> or 
		/// <see cref="DeclaredType"/>
		/// </summary>
		/// <param name="context">The current compile context</param>
		protected override Expression CreateExpressionBase(CompileContext context)
		{
			//have to pull the _factory member local otherwise it's seen as a member access, which
			//explodes in the full-blown Dynamic Assembly scenario, but it's private.
			//var factoryLocal = _factory;
			if(NullaryFactory != null)
				return Expression.Invoke(Expression.Constant(NullaryFactory));
			return Expression.Invoke(Expression.Constant(UnaryFactory), ExpressionHelper.RezolveContextParameterExpression);
		}

		/// <summary>
		/// Either <typeparamref name="T"/> or a base or interface of that type.
		/// </summary>
		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}

	/// <summary>
	/// Extension methods for creating <see cref="DelegateTarget{T}"/> instances.
	/// </summary>
	public static class DelegateTargetExtensions
	{
		/// <summary>
		/// Creates a <see cref="DelegateTarget{T}"/> using the delegate as the factory method to be executed.
		/// </summary>
		/// <typeparam name="T">The type returned by the factory when executed.</typeparam>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Optional override for the type exposed by the <see cref="DelegateTarget{T}"/>.
		/// See the <see cref="DelegateTarget{T}.DelegateTarget(Func{T}, Type)"/> constructor for more.</param>
		public static DelegateTarget<T> AsDelegateTarget<T>(this Func<T> factory, Type declaredType = null)
		{
			return new DelegateTarget<T>(factory, declaredType);
		}

		/// <summary>
		/// Creates a <see cref="DelegateTarget{T}"/> using the delegate as the factory method to be executed.
		/// </summary>
		/// <typeparam name="T">The type returned by the factory when executed.</typeparam>
		/// <param name="factory">The factory.</param>
		/// <param name="declaredType">Optional override for the type exposed by the <see cref="DelegateTarget{T}"/>.
		/// See the <see cref="DelegateTarget{T}.DelegateTarget(Func{RezolveContext, T}, Type)"/> constructor for more.</param>
		public static DelegateTarget<T> AsDelegateTarget<T>(this Func<RezolveContext, T> factory, Type declaredType = null)
		{
			return new DelegateTarget<T>(factory, declaredType);
		}
	}
}
