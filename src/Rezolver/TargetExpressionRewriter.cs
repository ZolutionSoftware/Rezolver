using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Used by <see cref="TargetBase"/> (and potentially your own targets)
	/// to convert <see cref="TargetExpression"/> instances which have been baked into
	/// expression trees (most likely by a <see cref="TargetAdapter"/>) into expressions
	/// for a given <see cref="CompileContext"/>.
	/// </summary>
	/// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
	/// <remarks>The <see cref="TargetBase"/> class always pushes expressions it receives
	/// from its <see cref="TargetBase.CreateExpressionBase(CompileContext)"/> abstract
	/// method through a rewrite - because if there are any non-standard expressions left,
	/// then compilation will not be possible.</remarks>
	public class TargetExpressionRewriter : ExpressionVisitor
    {
		readonly CompileContext _sourceCompileContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetExpressionRewriter"/> class for the
		/// given <paramref name="context"/>
		/// </summary>
		/// <param name="context">The compilation context.</param>
		public TargetExpressionRewriter(CompileContext context)
		{
			_sourceCompileContext = context;
		}
		/// <summary>
		/// Dispatches the expression to one of the more specialized visit methods in this class.
		/// </summary>
		/// <param name="node">The expression to visit.</param>
		public override Expression Visit(Expression node)
		{
			if (node != null)
			{
				if (node.NodeType == ExpressionType.Extension)
				{
					TargetExpression re = node as TargetExpression;
					if (re != null)
					{
						return re.Target.CreateExpression(_sourceCompileContext.New(re.Type));
					}
					//RezolveContextPlaceholderExpression pe = node as RezolveContextPlaceholderExpression;
					//if (pe != null)
					//	return _sourceCompileContext.RezolveContextExpression;
				}
			}
			return base.Visit(node);
		}
	}
}
