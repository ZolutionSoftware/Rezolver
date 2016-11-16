// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// A generic target for all expressions not explicitly supported by a particular target.
	/// 
	/// Enables more complex behaviours to be registered and used with the more formal
	/// <see cref="ITarget"/> implementations.
	/// </summary>
	public class ExpressionTarget : TargetBase
	{
		/// <summary>
		/// Gets the static expression represented by this target - if <c>null</c>, then 
		/// a factory is being used to produce the expression, which is available from
		/// the <see cref="ExpressionFactory"/> property.
		/// </summary>
		public Expression Expression { get; }

		private readonly Type _declaredType;//used only when a factory is used.

		/// <summary>
		/// Gets a factory which will be executed to obtain an expression when 
		/// <see cref="CreateExpressionBase(CompileContext)"/> is called.
		/// 
		/// If <c>null</c>, then a static expression will be used instead and is available
		/// from the <see cref="Expression"/> property.
		/// </summary>
		public Func<CompileContext, Expression> ExpressionFactory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionTarget"/> class.
		/// </summary>
		/// <param name="expression">Required. The static expression which will be returned by 
		/// <see cref="CreateExpressionBase(CompileContext)"/>.</param>
		public ExpressionTarget(Expression expression)
		{
			expression.MustNotBeNull(nameof(expression));
			Expression = expression;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionTarget"/> class.
		/// </summary>
		/// <param name="expressionFactory">Required. The factory delegate that will be 
		/// called to create the expression when <see cref="CreateExpressionBase(CompileContext)"/> is called.</param>
		/// <param name="declaredType">Required. Static type of all expressions that will be
		/// returned by <paramref name="expressionFactory"/>.</param>
		/// <remarks>Note that when you use a factory to produce a new expression on demand each time
		/// <see cref="CreateExpressionBase(CompileContext)"/> is called, the <paramref name="declaredType"/>
		/// is required to be the same for all.</remarks>
		public ExpressionTarget(Func<CompileContext, Expression> expressionFactory, Type declaredType)
		{
			expressionFactory.MustNotBeNull(nameof(expressionFactory));
			declaredType.MustNotBeNull(nameof(declaredType));
			ExpressionFactory = expressionFactory;
			_declaredType = declaredType;
		}

		/// <summary>
		/// Returns either the static <see cref="Expression"/> or the result of calling the 
		/// <see cref="ExpressionFactory"/> with the <paramref name="context"/>.  
		/// </summary>
		/// <param name="context">The current compile context</param>
		protected override Expression CreateExpressionBase(CompileContext context)
		{
			return Expression ?? ExpressionFactory(context);
		}

		/// <summary>
		/// Gets the type of <see cref="Expression"/> or the type that all expressions returned by the 
		/// <see cref="ExpressionFactory"/> are expected to be equal to.
		/// </summary>
		public override Type DeclaredType
		{
			get { return ExpressionFactory != null ? _declaredType : Expression.Type; }
		}
	}
}