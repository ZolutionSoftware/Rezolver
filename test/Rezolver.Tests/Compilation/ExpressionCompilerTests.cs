using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation
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
			foreach (var type in builderTypes)
			{
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

	abstract class TargetExpressionBuilderBase : IExpressionBuilder
	{
		/// <summary>
		/// Required for the scope tracking wrapping code.
		/// </summary>
		private static readonly MethodInfo ILifetimeScopeRezolver_AddObject = MethodCallExtractor.ExtractCalledMethod((IScopedContainer s) => s.AddToScope(null, null));
		private static readonly MethodInfo ILifetimeScopeRezolver_TrackIfScopedAndDisposableAndReturnGeneric =
			MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.TrackIfScopedAndDisposableAndReturn<object>(null, null)).GetGenericMethodDefinition();

		/// <summary>
		/// Abstract method called to create the expression - this is called by <see cref="CreateExpression"/> after the
		/// target type has been validated, if provided.
		/// 
		/// Note - if your implementation needs to support dynamic Resolve operations from the container that is passed
		/// to an IRezolver's Resolve method, you can use the <see cref="ExpressionHelper.DynamicRezolverParam"/> property,
		/// all the default implementations of this class (and others) use that by default.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns></returns>
		protected abstract Expression CreateExpressionBase(CompileContext context);

		/// <summary>
		/// This is called by <see cref="CreateExpression(CompileContext)"/> after the derived class generates its expression
		/// via a call to <see cref="CreateExpressionBase(CompileContext)"/> - unless <see cref="SuppressScopeTracking"/> is true either 
		/// on this object, or on the passed <paramref name="context"/>.
		/// 
		/// The purpose is to generate the code that will ensure that any instance produced will be tracked in a lifetime scope,
		/// if required.
		/// </summary>
		/// <param name="context">The current compile context.</param>
		/// <param name="expression">The code generated from the <see cref="CreateExpressionBase(CompileContext)"/> method, albeit
		/// possibly rewritten and optimised.</param>
		/// <remarks>
		/// By default, if there is a lifetime scope, then its <see cref="IScopedContainer.AddToScope(object, RezolveContext)"/> 
		/// method is called with the object that's produced by the target, before the object is returned.  If there is no scope, then 
		/// no tracking is performed.
		/// 
		/// Note that, also, by default, an object will only be tracked in the scope if it's <see cref="IDisposable"/>.
		/// 
		/// As mentioned in the summary, if you need to disable the automatic generation of this scope tracking code, then you
		/// can override the <see cref="SuppressScopeTracking"/> property, and return false.  It can also be suppressed on a per-compilation
		/// basis by setting the <see cref="CompileContext.SuppressScopeTracking"/> property of the <paramref name="context"/> to true.
		/// 
		/// This is something that the <see cref="ScopedTarget"/> does on its nested target, since by definition it generates
		/// its own explicit scope tracking code.
		/// 
		/// If the target simply needs to select a different scope from the current (at the time <see cref="IContainer.Resolve(RezolveContext)"/> 
		/// is called), then it can override the <see cref="CreateScopeSelectionExpression(CompileContext, Expression)"/> method.
		/// </remarks>
		/// <returns></returns>
		protected virtual Expression CreateScopeTrackingExpression(CompileContext context, Expression expression)
		{
			return Expression.Call(ILifetimeScopeRezolver_TrackIfScopedAndDisposableAndReturnGeneric.MakeGenericMethod(expression.Type),
				CreateScopeSelectionExpression(context, expression), expression);
		}

		/// <summary>
		/// Called to generate the expression that represents the argument that'll be passed to the 
		/// <see cref="IScopedContainer.AddToScope(object, RezolveContext)"/> method when an object is being tracked in a lifetime scope.  
		/// By default, the base implementation generates an expression that represents null - because usually there really is little point in 
		/// adding a specific context along with the object being tracked, unless you're also grabbing instances back out of the scope which isn't
		/// done by the base class behaviour by default.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		protected virtual Expression CreateRezolveContextExpressionForScopeAddCall(CompileContext context, Expression expression)
		{
			return Expression.Default(typeof(RezolveContext));
		}

		/// <summary>
		/// Called by <see cref="CreateScopeTrackingExpression(CompileContext, Expression)"/> to generate the code that selects the correct 
		/// scope instance that is to be used for scope tracking for the object produced by the code generated by 
		/// <see cref="CreateExpressionBase(CompileContext)"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		protected virtual Expression CreateScopeSelectionExpression(CompileContext context, Expression expression)
		{
			return context.ContextScopePropertyExpression;
		}

		public abstract bool CanBuild(ITarget target, CompileContext context);

		/// <summary>
		/// Virtual method implementing IRezolveTarget.CreateExpression.  Rather than overriding this method,
		/// your starting point is to implement the abstract method <see cref="CreateExpressionBase"/>.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns></returns>
		public Expression Build(ITarget target, CompileContext context)
		{
			target.MustNotBeNull(nameof(target));

			if (context.TargetType != null && !target.SupportsType(context.TargetType))
				throw new ArgumentException(String.Format(ExceptionResources.TargetDoesntSupportType_Format, context.TargetType),
				  nameof(context));

			if (!context.PushCompileStack(target))
				throw new InvalidOperationException(string.Format(ExceptionResources.CyclicDependencyDetectedInTargetFormat, target.GetType(), target.DeclaredType));

			try
			{
				var result = BuildBase(target, context);
				Type convertType = context.TargetType ?? target.DeclaredType;

				if (convertType == typeof(object) && TypeHelpers.IsValueType(result.Type)
				  || !TypeHelpers.IsAssignableFrom(convertType, target.DeclaredType)
				  || !TypeHelpers.IsAssignableFrom(convertType, result.Type))
					return Expression.Convert(result, convertType);

				result = new TargetExpressionRewriter(context).Visit(result);
				//if scope tracking isn't disabled, either by this target or at the compile context level, then we 
				//add the boilerplate to add this object produced to the current scope.
				//if (!target.SuppressScopeTracking && !context.SuppressScopeTracking)
				//{
				//	result = CreateScopeTrackingExpression(context, result);
				//}

				return result;
			}
			finally
			{
				context.PopCompileStack();
			}
		}

		protected abstract Expression BuildBase(ITarget target, CompileContext context);
	}

	class SingletonTargetExpressionBuilder : IExpressionBuilder<SingletonTarget>, ICompiledTarget
	{
		/// <summary>
		/// Used in ensuring the correct construction and scope tracking of a singleton instance.
		/// 
		/// Implements the ICompiledRezolveTarget interface for the single case of creating a lazy on demand (from the
		/// first RezolveContext that's passed to its GetObject method, and returning its value.
		/// </summary>
		private class SingletonTargetLazyInitialiser : ICompiledTarget
		{
			private Func<RezolveContext, Lazy<object>> _lazyFactory;
			private Lazy<object> _lazy;

			//internal Lazy<object> Lazy {  get { return _lazy; } }

			internal SingletonTargetLazyInitialiser(SingletonTarget target, Func<RezolveContext, Lazy<object>> lazyFactory)
			{
				_lazyFactory = lazyFactory;
			}

			/// <summary>
			/// Creates the singleton target's Lazy on demand if required, and then returns the object that the underlying
			/// compiled code produces.
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			object ICompiledTarget.GetObject(RezolveContext context)
			{
				if (_lazy == null)
				{
					//here - if we create more than one lazy, it's not a big deal.  *Executing* two
					//different ones is a big deal.
					Interlocked.CompareExchange(ref _lazy, _lazyFactory(context), null);
				}
				return _lazy.Value;
			}
		}

		private static readonly MethodInfo ICompiledRezolveTarget_GetObject = MethodCallExtractor.ExtractCalledMethod((ICompiledTarget t) => t.GetObject(null));
		private static readonly ConstructorInfo LazyObject_Ctor = MethodCallExtractor.ExtractConstructorCall(() => new Lazy<object>(() => (object)null));

		private readonly ConcurrentDictionary<Type, SingletonTargetLazyInitialiser> _initialisers = new ConcurrentDictionary<Type, SingletonTargetLazyInitialiser>();


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

	public class ExpressionCompilerTests
    {
	

    }
}
