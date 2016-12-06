using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{

	/// <summary>
	/// Class ExpressionCompiler.
	/// 
	/// This is internal for now - as the implementation will require a considerable amount of changes to the
	/// whole target/compilecontext stack.
	/// 
	/// Basically, the idea is to shift all of the expression generation stuff out of ITarget and make it a feature
	/// of a particular compiler only.  The compiler will resolve IExpressionBuilders using the container from the 
	/// compile context itself.  If that fails, then it will use its own internal container, comprising a default set of
	/// registrations based on all the known target types.
	/// </summary>
	internal class ExpressionCompiler : ITargetCompiler
	{
		//this is a copy of the same class from TargetDelegateCompiler
		private class DelegatingCompiledRezolveTarget : ICompiledTarget
		{
			private readonly Func<RezolveContext, object> _getObjectDelegate;

			public DelegatingCompiledRezolveTarget(Func<RezolveContext, object> getObjectDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
			}

			public object GetObject(RezolveContext context)
			{
				return _getObjectDelegate(context);
			}
		}

		public ICompiledTarget CompileTarget(ITarget target, CompileContext context)
		{
			//if the target is already a compiledTarget, then return its result.
			if (target is ICompiledTarget)
				return (ICompiledTarget)target;

			var builderTypes = new[] {
				typeof(IExpressionBuilder<>).MakeGenericType(target.GetType()),
				typeof(IExpressionBuilder)
			};

			IExpressionBuilder expressionBuilder = null;
			foreach (var type in builderTypes) {
				foreach (var expressionBuilderTarget in context.FetchAll(type))
				{
					//if the target is an ICompiledTarget, then we have to use that interface to get
					//an instance, even if the target also supports the IExpressionBuilder interface
					if (expressionBuilderTarget is ICompiledTarget)
					{
						expressionBuilder = ((ICompiledTarget)expressionBuilderTarget).GetObject(new RezolveContext(context.Container, type)) as IExpressionBuilder;
						if (expressionBuilder == null)
							continue;
					}
					else if ((expressionBuilder = expressionBuilderTarget as IExpressionBuilder) == null)
						throw new InvalidOperationException($"The target { expressionBuilderTarget } cannot be used by the compiler for the type { type } because it doesn't support either the ICompiledTarget or IExpressionBuilder interfaces");

					if (expressionBuilder.CanBuild(target, context))
						break;
					else
						expressionBuilder = null;
				}
				if (expressionBuilder != null)
					break;
			}

			if (expressionBuilder == null)
				throw new ArgumentException($"Unable to find an IExpressionBuilder for the target { target }", nameof(target));
			Expression<Func<RezolveContext, object>> lambda = GetLambdaForTarget(target, context, expressionBuilder);
			return CreateCompiledTargetForLambda(lambda);
		}

		private ICompiledTarget CreateCompiledTargetForLambda(Expression<Func<RezolveContext, object>> lambda)
		{
			return new DelegatingCompiledRezolveTarget(lambda.Compile());
		}

		private Expression<Func<RezolveContext, object>> GetLambdaForTarget(ITarget target, CompileContext context, IExpressionBuilder expressionBuilder)
		{
			var expression = expressionBuilder.Build(target, context);
			return Expression.Lambda<Func<RezolveContext, object>>(expression, context.RezolveContextExpression);
		}
	}

	interface IExpressionBuilder
	{
		bool CanBuild(ITarget target, CompileContext context);
		Expression Build(ITarget target, CompileContext context);
	}

	interface IExpressionBuilder<TTarget> : IExpressionBuilder
		where TTarget : ITarget
	{
		Expression Build(TTarget target, CompileContext context);
	}



	class SingletonTargetExpressionBuilder : IExpressionBuilder<SingletonTarget>, ICompiledTarget
	{
		Expression IExpressionBuilder.Build(ITarget target, CompileContext context)
		{
			return Build((SingletonTarget)target, context);
		}

		public Expression Build(SingletonTarget target, CompileContext context)
		{
			throw new NotImplementedException();
		}

		object ICompiledTarget.GetObject(RezolveContext context)
		{
			if (context.RequestedType == this.GetType() || context.RequestedType == typeof(IExpressionBuilder<SingletonTarget>))
				return this;
			throw new NotSupportedException("The requested type is not supported");
		}

		public bool CanBuild(ITarget target, CompileContext context)
		{
			return target is SingletonTarget;
		}
	}
}
