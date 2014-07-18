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

		public class DelegatingCompiledRezolveTarget/*<T>*/ : ICompiledRezolveTarget/*<T>*/
		{
			private readonly Func<object> _getObjectDelegate;
			private readonly Func<IRezolverContainer, object> _getObjectDynamicDelegate;
			//private readonly Func<T> _getObjectStrongDelegate;
			//private readonly Func<IRezolverContainer, T> _getObjectStrongDynamicDelegate;

			public DelegatingCompiledRezolveTarget(Func<object> getObjectDelegate,
				Func<IRezolverContainer, object> getObjectDynamicDelegate/*,
				Func<T> getObjectStrongDelegate, Func<IRezolverContainer, T> getObjectStrongDynamicDelegate*/)
			{
				_getObjectDelegate = getObjectDelegate;
				_getObjectDynamicDelegate = getObjectDynamicDelegate;
				//_getObjectStrongDelegate = getObjectStrongDelegate;
				//_getObjectStrongDynamicDelegate = getObjectStrongDynamicDelegate;
			}

			public object GetObject()
			{
				return _getObjectDelegate();
			}

			public object GetObjectDynamic(IRezolverContainer dynamicContainer)
			{
				return _getObjectDynamicDelegate(dynamicContainer);
			}

			//public T GetObject()
			//{
			//	return _getObjectStrongDelegate();
			//}

			//public T GetObjectDynamic(IRezolverContainer dynamicContainer)
			//{
			//	return _getObjectStrongDynamicDelegate(dynamicContainer);
			//}
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
		/// <param name="containerScope"></param>
		/// <param name="dynamicContainerExpression"></param>
		/// <param name="targetStack"></param>
		/// <returns></returns>
		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> targetStack = null)
		{
			//it doesn't have to be lightning quick, this.
			return new DelegatingCompiledRezolveTarget(
				CompileStatic(target, containerScope,  targetStack: targetStack),
				CompileDynamic(target, containerScope, dynamicContainerExpression ?? ExpressionHelper.DynamicContainerParam, targetStack: targetStack)/*,
				CompileStaticGeneric.MakeGenericMethod(target.DeclaredType)
					.Invoke(this, new object[] {target, containerScope, targetStack}),
				CompileDynamicGeneric.MakeGenericMethod(target.DeclaredType)
					.Invoke(this, new object[] {target, containerScope, dynamicContainerExpression ?? ExpressionHelper.DynamicContainerParam, targetStack})*/
				);
		}

		private Func<object> CompileStatic(IRezolveTarget target, IRezolverContainer containerScope, Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<object>>(target.CreateExpression(containerScope, targetType: typeof (object),
					currentTargets: targetStack));
			Debug.WriteLine("Compiling Func<object> from static lambda {0} for target type {1}", expression, targetType != null ? targetType.Name : "[null]");
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<object>>(target.CreateExpression(containerScope, targetType: typeof(object), currentTargets: targetStack)).Compile();
#endif
		}

		private Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<TTarget>>(target.CreateExpression(containerScope, targetType: typeof (TTarget),
					currentTargets: targetStack));
			Debug.WriteLine("Compiling Func<{0}> from static lambda {1}", typeof(TTarget), expression);
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<TTarget>>(target.CreateExpression(containerScope, targetType: typeof(TTarget),
					currentTargets: targetStack)).Compile();
#endif
		}

		private Func<IRezolverContainer, object> CompileDynamic(IRezolveTarget target, IRezolverContainer containerScope, ParameterExpression dynamicContainerExpression, 
			Type targetType = null, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression =
				Expression.Lambda<Func<IRezolverContainer, object>>(
					target.CreateExpression(containerScope, targetType: typeof (object),
						dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression ?? ExpressionHelper.DynamicContainerParam);
			Debug.WriteLine("Compiling Func<IRezolverContainer, object> from dynamic lambda {0} for target type {1}", expression, targetType != null ? targetType.Name : "[null]");
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<IRezolverContainer, object>>(target.CreateExpression(containerScope, targetType: typeof(object),
					dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression).Compile();
#endif
		}

		private Func<IRezolverContainer, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack = null)
		{
#if DEBUG
			var expression = Expression.Lambda<Func<IRezolverContainer, TTarget>>(target.CreateExpression(containerScope,
				targetType: typeof (TTarget),
				dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression ?? ExpressionHelper.DynamicContainerParam);
			Debug.WriteLine("Compiling Func<IRezolverContainer, {0}> from dynamic lambda {1}", typeof(TTarget), expression);
			return expression.Compile();
#else
			return
				Expression.Lambda<Func<IRezolverContainer, TTarget>>(target.CreateExpression(containerScope,
					targetType: typeof (TTarget),
					dynamicContainerExpression: dynamicContainerExpression, currentTargets: targetStack), dynamicContainerExpression).Compile();
#endif
		}
	}
}