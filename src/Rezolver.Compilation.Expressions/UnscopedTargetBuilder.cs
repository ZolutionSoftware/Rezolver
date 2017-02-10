using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Builder for the <see cref="UnscopedTarget"/>.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.Targets.UnscopedTarget}" />
	public class UnscopedTargetBuilder : ExpressionBuilderBase<UnscopedTarget>
    {
		protected override Expression ApplyScoping(ScopeActivationBehaviour scopeBehaviour, Expression builtExpression, ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//forcibly disable any scoping code for the expression that's built if somehow it sneaks through
			return builtExpression;
		}

		protected override Expression Build(UnscopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//force scoping off for the inner target
			return compiler.Build(target.Inner, context.NewContext(scopeBehaviourOverride: ScopeActivationBehaviour.None));
		}
	}
}
