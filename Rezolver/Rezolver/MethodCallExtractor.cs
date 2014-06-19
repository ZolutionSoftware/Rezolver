using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public sealed class MethodCallExtractor : ExpressionVisitor
	{
		private MethodCallExpression _callExpr;

		public MethodCallExpression CallExpression
		{
			get { return _callExpr; }
		}

		public MethodInfo CalledMethod
		{
			get
			{
				return _callExpr != null ? _callExpr.Method : null;
			}
		}
		private MethodCallExtractor(Expression e)
		{
			Visit(e);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (_callExpr == null)
				_callExpr = node;
			return base.VisitMethodCall(node);
		}

		public static MethodInfo ExtractCalledMethod<T>(Expression<Action<T>> expr)
		{
			var visitor = new MethodCallExtractor(expr);

			return visitor.CalledMethod;
		}

		public static MethodInfo ExtractCalledMethod(Expression<Action> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledMethod;
		}
	}
}