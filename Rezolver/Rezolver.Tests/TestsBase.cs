using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		//protected Func<IRezolverScope, TTarget> ToFunc(Expression e) 
		protected static Func<T> CompileTargetExpression<T>(IRezolveTarget target, IRezolverContainer scope = null)
		{
			var lambda = Expression.Lambda<Func<T>>(target.CreateExpression(scope ?? Mock.Of<IRezolverContainer>(),targetType: typeof(T)));
			Debug.WriteLine("TestsBase is compiling lambda {0} for target type {1}", lambda, typeof(T));
			return lambda.Compile();
		}

		protected static Delegate CompileTargetExpression(IRezolveTarget target, IRezolverContainer scope = null)
		{
			var lambda = Expression.Lambda(target.CreateExpression(scope ?? Mock.Of<IRezolverContainer>()));
			Debug.WriteLine("TestsBase is compiling lambda {0}", lambda);
			return lambda.Compile();
		}

		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolverContainer scope = null)
		{
			return CompileTargetExpression<T>(target, scope)();
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverContainer scope = null)
		{
			return CompileTargetExpression(target, scope).DynamicInvoke();
		}
	}
}