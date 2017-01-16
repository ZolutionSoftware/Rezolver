using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	public class ExpressionTargetBuilder : ExpressionBuilderBase<ExpressionTarget>
	{
		protected override Expression Build(ExpressionTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			return target.ExpressionFactory != null ?
				target.ExpressionFactory(context) : target.Expression;
		}
	}
}
