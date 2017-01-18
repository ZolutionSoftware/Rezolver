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
	/// An <see cref="IExpressionBuilder"/> specialised for building the expression for the <see cref="DefaultTarget"/> target.
	/// 
	/// Essentially, it just calls <see cref="Expression.Default(Type)"/> for the <see cref="DefaultTarget.DeclaredType"/>.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.DefaultTarget}" />
	public class DefaultTargetBuilder : ExpressionBuilderBase<DefaultTarget>
	{
		protected override Expression Build(DefaultTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			return Expression.Default(target.DeclaredType);
		}
	}
}
