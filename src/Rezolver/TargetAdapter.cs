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
		/// <summary>
		/// The work horse for the TargetAdapter
		/// </summary>
		/// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
		private class TargetAdapterVisitor : ExpressionVisitor
		{
			private ITargetAdapter _adapter;
			public TargetAdapterVisitor(ITargetAdapter adapter)
			{
				_adapter = adapter;
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

			protected override Expression VisitConstant(ConstantExpression node)
			{
				return new TargetExpression(new ObjectTarget(node.Value, node.Type));
			}

			protected override Expression VisitNew(NewExpression node)
			{
				return new TargetExpression(ConstructorTarget.FromNewExpression(node.Type, node, _adapter));
			}

			protected override Expression VisitMemberInit(MemberInitExpression node)
			{
				var constructorTarget = ConstructorTarget.FromNewExpression(node.Type, node.NewExpression, _adapter);
				return new TargetExpression(new ExpressionTarget(c =>
				{
					var ctorTargetExpr = constructorTarget.CreateExpression(c.New(node.Type));

					//the goal here, then, is to find the new expression for this type and replace it 
					//with a memberinit equivalent to the one we visited.  Although the constructor target produces 
					//a NewExpression, it isn't going to be the root expression, because of the scoping boilerplate 
					//that is put around nearly all expressions produced by RezolveTargetBase implementations. 
					var rewriter = new NewExpressionMemberInitRewriter(node.Type, node.Bindings.Select(mb => VisitMemberBinding(mb)));
					return rewriter.Visit(ctorTargetExpr);
				}, node.Type));
			}

			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				Expression body = node.Body;
				try
				{
					ParameterExpression rezolveContextParam = node.Parameters.SingleOrDefault(p => p.Type == typeof(RezolveContext));
					//if the lambda had a parameter of the type RezolveContext, swap it for the 
					//RezolveContextParameterExpression parameter expression that all the internal
					//components use when building expression trees from targets.
					if (rezolveContextParam != null && rezolveContextParam != ExpressionHelper.RezolveContextParameterExpression)
						body = new ExpressionSwitcher(new[] {
							new ExpressionReplacement(rezolveContextParam, ExpressionHelper.RezolveContextParameterExpression)
						}).Visit(body);
				}
				catch (InvalidOperationException ioex)
				{
					//throw by the SingleOrDefault call inside the Try.
					throw new ArgumentException($"The lambda expression { node } is not supported - it has multiple RezolveContext parameters, and only a maximum of one is allowed", nameof(node), ioex);
				}
				var variables = node.Parameters.Where(p => p.Type != typeof(RezolveContext)).ToArray();
				//if we have lambda parameters which need to be converted to block variables which are resolved
				//by assignment (dynamic service location I suppose you'd call it) then we need to wrap everything
				//in a block expression.
				if (variables.Length != 0)
				{
					return Expression.Block(node.Body.Type,
						//all parameters from the Lambda, except one typed as RezolveContext, are fed into the new block as variables
						variables,
						//start the block with a run of assignments for all the parameters of the original lambda
						//with services resolved from the container
						variables.Select(p => Expression.Assign(p, new TargetExpression(new RezolvedTarget(p.Type)))).Concat(
							new[] {
								//and then concatenate the original body of the Lambda, which might have had
								//any references to a RezolveContext parameter switched for the global RezolveContextParameterExpression
								base.Visit(body)
							}
						)
					);
				}
				else
					return base.Visit(body);
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				var rezolvedType = ExtractRezolveCallType(node);
				if (rezolvedType != null)
					return new TargetExpression(new RezolvedTarget(rezolvedType));
				return base.VisitMethodCall(node);
			}
		}

		internal static readonly MethodInfo[] RezolveMethods =
		{
			MethodCallExtractor.ExtractCalledMethod(() => Functions.Resolve<int>()).GetGenericMethodDefinition()
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



		/// <summary>
		/// Creates the target.
		/// </summary>
		/// <param name="expression">The expression.</param>
		public ITarget CreateTarget(Expression expression)
		{
			var result = new TargetAdapterVisitor(this).Visit(expression);
			if (result is TargetExpression)
				return ((TargetExpression)result).Target;
			else if (result != null)
				return new ExpressionTarget(result);
			else
				throw new ArgumentException($"Unable to convert the expression { expression } to an ITarget", nameof(expression));
		}
	}
}