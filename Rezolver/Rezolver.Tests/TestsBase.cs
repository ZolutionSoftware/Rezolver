using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		//protected Func<IRezolverScope, TTarget> ToFunc(Expression e) 
		protected static Func<T> CompileTargetExpression<T>(IRezolveTarget target, IRezolverScope scope = null)
		{
			var lambda = Expression.Lambda<Func<T>>(target.CreateExpression(scope ?? Mock.Of<IRezolverScope>(),targetType: typeof(T)));
			Debug.WriteLine("TestsBase is compiling lambda {0} for target type {1}", lambda, typeof(T));
			return lambda.Compile();
		}

		protected static Delegate CompileTargetExpression(IRezolveTarget target, IRezolverScope scope = null)
		{
			var lambda = Expression.Lambda(target.CreateExpression(scope ?? Mock.Of<IRezolverScope>()));
			Debug.WriteLine("TestsBase is compiling lambda {0}", lambda);
			return lambda.Compile();
		}

		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolverScope scope = null)
		{
			return CompileTargetExpression<T>(target, scope)();
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverScope scope = null)
		{
			return CompileTargetExpression(target, scope).DynamicInvoke();
		}
	}
}