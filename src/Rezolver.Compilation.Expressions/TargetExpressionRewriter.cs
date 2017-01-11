using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
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
	internal class TargetExpressionRewriter : ExpressionVisitor
    {
		readonly CompileContext _sourceCompileContext;
		readonly IExpressionCompiler _compiler;
		/// <summary>
		/// Initializes a new instance of the <see cref="TargetExpressionRewriter"/> class for the
		/// given <paramref name="context"/>
		/// </summary>
		/// <param name="context">The compilation context.</param>
		public TargetExpressionRewriter(IExpressionCompiler compiler, CompileContext context)
		{
			_compiler = compiler;
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
					TargetExpression te = node as TargetExpression;
					if (te != null)
						return _compiler.Build(te.Target, _sourceCompileContext.New(te.Type));
				}
			}
			return base.Visit(node);
		}
	}
}
