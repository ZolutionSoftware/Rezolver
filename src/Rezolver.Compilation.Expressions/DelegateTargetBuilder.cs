﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Specialised builder for the <see cref="DelegateTarget"/> class and all its derivatives.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.DelegateTarget}" />
	public class DelegateTargetBuilder : ExpressionBuilderBase<DelegateTarget>
	{
		/// <summary>
		/// Builds an expression from the specified target for the given <see cref="T:Rezolver.CompileContext" />
		/// OVerride this to implement the compilation for your target type.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		/// <exception cref="NotImplementedException"></exception>
		protected override Expression Build(DelegateTarget target, CompileContext context, IExpressionCompiler compiler)
		{
			var bindings = ParameterBinding.BindWithRezolvedArguments(target.Factory.GetMethodInfo());
			return Expression.Invoke(Expression.Constant(target.Factory),
				bindings.Select(b => b.Parameter.ParameterType == typeof(RezolveContext) ?
					ExpressionHelper.RezolveContextParameterExpression
					: compiler.Build(b.Target, context.New(b.Parameter.ParameterType))));
		}
	}
}
