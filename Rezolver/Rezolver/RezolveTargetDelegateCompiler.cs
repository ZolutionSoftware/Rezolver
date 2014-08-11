using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel.Channels;

namespace Rezolver
{
	public class RezolveTargetDelegateCompiler : IRezolveTargetCompiler
	{
		public static readonly IRezolveTargetCompiler Default = new RezolveTargetDelegateCompiler();

		public class DelegatingCompiledRezolveTarget : ICompiledRezolveTarget
		{
			private readonly Func<object> _getObjectDelegate;
			private readonly Func<IRezolver, object> _getObjectDynamicDelegate;

			public DelegatingCompiledRezolveTarget(Func<object> getObjectDelegate,
				Func<IRezolver, object> getObjectDynamicDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
				_getObjectDynamicDelegate = getObjectDynamicDelegate;
			}

			public object GetObject()
			{
				return _getObjectDelegate();
			}

			public object GetObjectDynamic(IRezolver @dynamic)
			{
				return _getObjectDynamicDelegate(dynamic);
			}
		}

		private static readonly MethodInfo CompileStaticGeneric = typeof (RezolveTargetDelegateCompiler).GetMethods(BindingFlags.Instance |
																								BindingFlags.NonPublic)
																								.SingleOrDefault(mi => mi.Name == "CompileStatic" && mi.IsGenericMethodDefinition);
		private static readonly MethodInfo CompileDynamicGeneric = typeof(RezolveTargetDelegateCompiler).GetMethods(BindingFlags.Instance |
																								BindingFlags.NonPublic)
																								.SingleOrDefault(mi => mi.Name == "CompileDynamic" && mi.IsGenericMethodDefinition);

		/// <summary>
		/// Creates and returns a compiled target for the passed rezolve target.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="scope"></param>
		/// <param name="dynamicRezolverExpression"></param>
		/// <param name="targetStack"></param>
		/// <returns></returns>
		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolver scope,
			ParameterExpression dynamicRezolverExpression = null, Stack<IRezolveTarget> targetStack = null)
		{
			//it doesn't have to be lightning quick, this.
			return new DelegatingCompiledRezolveTarget(
				CompileStatic(target, scope,  targetStack: targetStack),
				CompileDynamic(target, scope, dynamicRezolverExpression ?? ExpressionHelper.DynamicRezolverParam, targetStack: targetStack)
				);
		}

		private Func<object> CompileStatic(IRezolveTarget target, IRezolver scope, Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<object>>(target.CreateExpression(scope, targetType: typeof (object),
					currentTargets: targetStack));
			Debug.WriteLine("Compiling Func<object> from static lambda {0} for target type {1}", expression, "System.Object");
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<object>>(target.CreateExpression(containerScope, targetType: typeof(object), currentTargets: targetStack)).Compile();
#endif
		}

		private Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolver scope, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<TTarget>>(target.CreateExpression(scope, targetType: typeof (TTarget),
					currentTargets: targetStack));
			Debug.WriteLine("Compiling Func<{0}> from static lambda {1}", typeof(TTarget), expression);
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<TTarget>>(target.CreateExpression(scope, targetType: typeof(TTarget),
					currentTargets: targetStack)).Compile();
#endif
		}

		private Func<IRezolver, object> CompileDynamic(IRezolveTarget target, IRezolver scope, ParameterExpression dynamicRezolverExpression, 
			Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<IRezolver, object>>(
					target.CreateExpression(scope, targetType: typeof (object),
						dynamicRezolverExpression: dynamicRezolverExpression, currentTargets: targetStack), dynamicRezolverExpression ?? ExpressionHelper.DynamicRezolverParam);
			Debug.WriteLine("Compiling Func<IRezolver, object> from dynamic lambda {0} for target type {1}", expression, targetType != null ? targetType.Name : "[null]");
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<IRezolver, object>>(target.CreateExpression(scope, targetType: typeof(object),
					dynamicRezolverExpression: dynamicRezolverExpression, currentTargets: targetStack), dynamicRezolverExpression).Compile();
#endif
		}

		private Func<IRezolver, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolver scope,
			ParameterExpression dynamicRezolverExpression, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression = Expression.Lambda<Func<IRezolver, TTarget>>(target.CreateExpression(scope,
				targetType: typeof (TTarget),
				dynamicRezolverExpression: dynamicRezolverExpression, currentTargets: targetStack), dynamicRezolverExpression ?? ExpressionHelper.DynamicRezolverParam);
			Debug.WriteLine("Compiling Func<IRezolver, {0}> from dynamic lambda {1}", typeof(TTarget), expression);
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<IRezolver, TTarget>>(target.CreateExpression(scope,
					targetType: typeof (TTarget),
					dynamicRezolverExpression: dynamicRezolverExpression, currentTargets: targetStack), dynamicRezolverExpression).Compile();
#endif
		}
	}
}