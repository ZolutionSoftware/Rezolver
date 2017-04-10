// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
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
	public class DelegateTargetBuilder : ExpressionBuilderBase<DelegateTarget>
	{
		/// <summary>
		/// Builds an expression from the specified target for the given <see cref="IExpressionCompileContext" />
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		/// <exception cref="NotImplementedException"></exception>
		protected override Expression Build(DelegateTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			var bindings = ParameterBinding.BindWithRezolvedArguments(target.Factory.GetMethodInfo());
			return Expression.Invoke(Expression.Constant(target.Factory),
				bindings.Select(b => b.Parameter.ParameterType == typeof(IResolveContext) ?
					context.ResolveContextExpression
					: compiler.Build(b.Target, context.NewContext(b.Parameter.ParameterType))));
		}
	}
}
