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
	/// An <see cref="IExpressionBuilder"/> specialised for building expressions for <see cref="ScopedTarget"/> targets.
	/// </summary>
	public class ScopedTargetBuilder : ExpressionBuilderBase<ScopedTarget>
	{
		private ConstructorInfo _argExceptionCtor = 
			MethodCallExtractor.ExtractConstructorCall(() => new ArgumentException("", ""));

		protected override Expression Build(ScopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//all we need to do is force the inner target's scope behaviour to None - and this builder's
			//base code will ensure that the whole resulting expression is converted into an explicitly scoped one

			//note that this scope deactivation is only in place for this one target - if it has any child targets then
			//scoping behaviour for those returns to normal if compiled with a new context (which they always should be)

			return compiler.Build(target.InnerTarget, context.NewContext(scopeBehaviourOverride: ScopeActivationBehaviour.None));
		}
	}
}
