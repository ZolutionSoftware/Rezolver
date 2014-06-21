using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolveTargetAdapterTests
	{
		[TestMethod]
		public void ShouldAdaptToObjectTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget(Expression.Constant(0)), typeof(ObjectTarget));
		}

		[TestMethod]
		public void ShouldAdaptToConstructorTarget()
		{

		}
	}

	public class RezolveTargetAdapter : ExpressionVisitor, IRezolveTargetAdapter
	{
		public IRezolveTarget ConvertToTarget(Expression expression)
		{
			var visitor = new TargetFactoryVisitor();
			return visitor.BuildTarget(expression);
		}

		public class TargetFactoryVisitor : ExpressionVisitor
		{
			public TargetFactoryVisitor()
			{

			}

			public IRezolveTarget BuildTarget(Expression expression)
			{
				var result = Visit(expression) as RezolveTargetExpression;
				if (result != null)
					return result.Target;
				return null;
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				return new RezolveTargetExpression(new ObjectTarget(node.Value, node.Type));
			}

			protected override Expression VisitNew(NewExpression node)
			{
				var parameters = node.Constructor.GetParameters();
				return new RezolveTargetExpression(new ConstructorTarget(node.Type, node.Constructor,
					node.Arguments.Select((pExp, i) => new ParameterBinding(parameters[i], BuildTarget(node))).ToArray()));
			}

			public override Expression Visit(Expression node)
			{
				var result = base.Visit(node);
				return result;
			}
		}
	}

	/// <summary>
	/// Makes it possible to mix expressions and targets.
	/// 
	/// Note that this *fake* expression typee does not compile.
	/// </summary>
	public class RezolveTargetExpression : Expression
	{
		private readonly IRezolveTarget _target;

		public RezolveTargetExpression(IRezolveTarget target)
		{
			_target = target;
		}

		public IRezolveTarget Target
		{
			get { return _target; }
		}
	}
}
