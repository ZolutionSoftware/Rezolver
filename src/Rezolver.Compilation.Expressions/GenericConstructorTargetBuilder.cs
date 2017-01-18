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
	/// An <see cref="IExpressionBuilder"/> specialised for building expressions for <see cref="GenericConstructorTarget"/> targets.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.GenericConstructorTarget}" />
	public class GenericConstructorTargetBuilder : ExpressionBuilderBase<GenericConstructorTarget>
	{
		/// <summary>
		/// Obtains the bound target for the <paramref name="target"/> passed (by calling 
		/// <see cref="GenericConstructorTarget.Bind(IExpressionCompileContext)"/>, and passes it to the 
		/// <paramref name="compiler"/> to have an expression built for it.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		protected override Expression Build(GenericConstructorTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//simply bind the generic target to the context, obtain the target that is produced
			//and then build it.
			return compiler.Build(target.Bind(context), context);
		}
	}
}
