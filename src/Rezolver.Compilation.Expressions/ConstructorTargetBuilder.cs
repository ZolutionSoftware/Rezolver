// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised for building the target <see cref="ConstructorTarget"/>
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.ConstructorTarget}" />
	public class ConstructorTargetBuilder : ExpressionBuilderBase<ConstructorTarget>
	{
		/// <summary>
		/// Override of <see cref="ExpressionBuilderBase{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)"/>
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		protected override Expression Build(ConstructorTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			return Build(target.Bind(context), context, compiler);
		}

		/// <summary>
		/// Builds an expression for the specified <see cref="ConstructorBinding"/>.
		/// 
		/// Called by <see cref="Build(ConstructorTarget, IExpressionCompileContext, IExpressionCompiler)"/>
		/// </summary>
		/// <param name="binding">The binding.</param>
		/// <param name="context">The context.</param>
		/// <remarks>The returned expression will either be a NewExpression or a MemberInitExpression</remarks>
		protected virtual Expression Build(ConstructorBinding binding, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			var newExpr = Expression.New(binding.Constructor, 
				binding.BoundArguments.Select(
					a => compiler.Build(a.Target, context.NewContext(a.Parameter.ParameterType))));

			if (binding.MemberBindings.Length == 0)
				return newExpr;
			else
				return Expression.MemberInit(newExpr, 
					binding.MemberBindings.Select(
						mb => Expression.Bind(mb.Member, 
						compiler.Build(mb.Target, context.NewContext(mb.MemberType)))));
		}
	}
}
