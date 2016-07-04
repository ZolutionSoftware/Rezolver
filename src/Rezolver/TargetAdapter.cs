// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
	/// Default implementation of the <see cref="ITargetAdapter"/> interface.
	/// 
	/// Also an ExpressionVisitor.
	/// 
	/// Also, its <see cref="Default" /> property serves as the reference to the default adapter used by the 
	/// system to convert expressions into IRezolveTarget instances.
	/// 
	/// This class cannot be created directly - it is a singleton accessed through the <see cref="Instance" />
	/// property.  You can inherit from this class, however, to serve as the basis for your own implementation
	/// of <see cref="ITargetAdapter"/>.
	/// </summary>
	public class TargetAdapter : ExpressionVisitor, ITargetAdapter
	{
		internal static readonly MethodInfo[] RezolveMethods =
		{
						MethodCallExtractor.ExtractCalledMethod((RezolveContextExpressionHelper helper) => helper.Resolve<int>()).GetGenericMethodDefinition()
				};

		/// <summary>
		/// a lazy that creates one instance of this class to be used as the ultimate default 
		/// </summary>
		private static readonly Lazy<ITargetAdapter> _instance =
				new Lazy<ITargetAdapter>(() => new TargetAdapter());

		/// <summary>
		/// The one and only instance of the RezolveTargetAdapter class
		/// </summary>
		public static ITargetAdapter Instance
		{
			get { return _instance.Value; }
		}

		private static ITargetAdapter _default = _instance.Value;

		/// <summary>
		/// The default IRezolveTargetAdapter to be used in converting expressions to IRezolveTarget instances.
		/// By default, this is initialised to a single instance of the <see cref="TargetAdapter"/> class.
		/// </summary>
		public static ITargetAdapter Default
		{
			get { return _default; }
			set
			{
				value.MustNotBeNull("value");

				_default = value;
			}
		}

		/// <summary>
		/// Protected constructor ensuring that new instances can only be created through inheritance.
		/// </summary>
		protected TargetAdapter()
		{

		}

		internal Type ExtractRezolveCallType(Expression e)
		{
			var methodExpr = e as MethodCallExpression;

			if (methodExpr == null || !methodExpr.Method.IsGenericMethod)
				return null;

			var match = RezolveMethods.SingleOrDefault(m => m.Equals(methodExpr.Method.GetGenericMethodDefinition()));

			if (match == null)
				return null;

			return methodExpr.Method.GetGenericArguments()[0];
		}

		public ITarget CreateTarget(Expression expression)
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
			return new RezolveTargetExpression(ConstructorTarget.FromNewExpression(node.Type, node, this));
		}

		protected override Expression VisitMemberInit(MemberInitExpression node)
		{
			var constructorTarget = ConstructorTarget.FromNewExpression(node.Type, node.NewExpression, this);
			return new RezolveTargetExpression(new ExpressionTarget(c =>
			{
				var ctorTargetExpr = constructorTarget.CreateExpression(c.New(node.Type));

							//the goal here, then, is to find the new expression for this type and replace it 
							//with a memberinit equivalent to the one we visited.  Although the constructor target produces 
							//a NewExpression, it isn't going to be the root expression, because of the scoping boilerplate 
							//that is put around nearly all expressions produced by RezolveTargetBase implementations. 
							var rewriter = new NewExpressionMemberInitRewriter(node.Type, node.Bindings.Select(mb => VisitMemberBinding(mb)));
				return rewriter.Visit(ctorTargetExpr);
			}, node.Type, this));
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
		{
			return base.VisitMemberAssignment(node);
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
		{
			return base.VisitMemberListBinding(node);
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
		{
			return base.VisitMemberMemberBinding(node);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (node.Type == typeof(RezolveContextExpressionHelper))
				return new RezolveContextPlaceholderExpression(node);
			return base.VisitParameter(node);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			//we can't do anything special with lambdas - we just work over the body.  This enables
			//us to feed lambdas from code (i.e. compiler-generated expression trees) just as if we
			//were passing hand-built expressions.
			return base.Visit(node.Body);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			//TODO: no string parameter here -needs to be reinstated.
			var rezolvedType = ExtractRezolveCallType(node);
			if (rezolvedType != null)
				return new RezolveTargetExpression(new RezolvedTarget(rezolvedType));
			return base.VisitMethodCall(node);
		}

		public override Expression Visit(Expression node)
		{
			var result = base.Visit(node);
			if (result != null && !(result is RezolveTargetExpression))
				return new RezolveTargetExpression(new ExpressionTarget(result));
			return result;
		}
	}
}