using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	/// <summary>
	/// the conditions under which this will rewrite an expression are very tight - only those
	/// conditionals which have identical (down to reference-level) tests will be selected for
	/// rewriting.
	/// 
	/// The rewriting, therefore, is only really made possible when a target makes use of the 
	/// shared expressions feature of the compile context.
	/// </summary>
	public class ConditionalRewriter : ExpressionVisitor
	{
		private enum RewriteStages
		{
			NotRun,
			GatheringConditionals,
			RewritingConditionals
		}

		private class ConditionalExpressionInfo : IEquatable<ConditionalExpressionInfo>
		{
			public ConditionalExpression Expression;
			public Stack<Expression> Hierarchy;

			//equality is exceptionally simple - only performs reference equality check.
			//this works only because the specific scenario that this rewriter is designed to
			//handle relates to the RezolvedTarget - which uses shared expressions.
			public bool Equals(ConditionalExpressionInfo other)
			{
				return object.ReferenceEquals(Expression.Test, other.Expression.Test);
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as ConditionalExpressionInfo);
			}

			public override int GetHashCode()
			{
				return Expression.Test.GetHashCode();
			}
		}

		private class ConditionalExpressionGroupInfo
		{
			public IGrouping<ConditionalExpressionInfo, ConditionalExpressionInfo> Group;
			public Expression CommonParent;
		}

		private Stack<Expression> _currentStack = new Stack<Expression>();
		private List<ConditionalExpressionInfo> _allConditionals = new List<ConditionalExpressionInfo>();
		private ConditionalExpressionGroupInfo[] _groupedConditionals = null;
		private RewriteStages _currentStage = RewriteStages.NotRun;
		public ConditionalRewriter()
		{

		}

		public Expression Rewrite(Expression e)
		{
			_currentStage = RewriteStages.GatheringConditionals;
			var result = Visit(e);
			//if there are no conditionals, or only one, then there's no benefit to rewriting.
			if (_allConditionals.Count <= 1)
				return result;
			var grouped = _allConditionals.GroupBy(i => i).ToArray();

			if (grouped.Length == _allConditionals.Count)
				return result;	//no rewriting to do here, all conditional tests are unique
 
			//now we have to find whether this group has a shared parent expression that we can rewrite
			//this involves finding the bottom-most expression which is present in all the stacks that
			//we gathered.  We won't remove that expression, we're just going to push it down and clone it 
 			//as a new pair of iffalse and iftrue branches in a new conditional expression.
			_groupedConditionals = (from grp in grouped
													 from conditional in grp
													 from expr in conditional.Hierarchy
													 let otherHierarchies = (from conditional2 in grp.Skip(1)
																									 select conditional2.Hierarchy)
													 where otherHierarchies.All(h => h.Any(expr2 => object.ReferenceEquals(expr, expr2)))
													 select new ConditionalExpressionGroupInfo { Group = grp, CommonParent = expr }).ToArray();
			return result;
		}

		public override Expression Visit(Expression node)
		{
			if (_currentStage == RewriteStages.GatheringConditionals)
			{
				_currentStack.Push(node);
				var result = base.Visit(node);
				_currentStack.Pop();
				return result;
			}
			else
				return base.Visit(node);
		}

		protected override Expression VisitConditional(ConditionalExpression node)
		{
			if (_currentStage == RewriteStages.GatheringConditionals)
			{
				//remember that the stack here will have this node as the most recently added object
				_allConditionals.Add(new ConditionalExpressionInfo() { Expression = node, Hierarchy = new Stack<Expression>(_currentStack) });
				return base.VisitConditional(node);
			}
			else
				return base.VisitConditional(node);
		}
	}

	[TestClass]
	public class ConditionalRewriterTest
	{
		public class TypeWith2ConstructorArgs
		{
			public TypeWith2ConstructorArgs(int a, string b) 
			{ 
			}
		}

		[TestMethod]
		public void TestMethod1()
		{
			DefaultRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			CompileContext sharedContext = new CompileContext(rezolver);

			rezolver.Register((1).AsObjectTarget());
			rezolver.Register("hello world".AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<TypeWith2ConstructorArgs>());
			var ctorTarget = rezolver.Fetch(typeof(TypeWith2ConstructorArgs));
			var expression = ctorTarget.CreateExpression(sharedContext);

			ConditionalRewriter rewriter = new ConditionalRewriter();
			var rewritten = rewriter.Rewrite(expression);

			Assert.IsNotNull(rewritten);
		}
	}
}
