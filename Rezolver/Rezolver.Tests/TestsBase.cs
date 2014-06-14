using System;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		//protected Func<IRezolverScope, TTarget> ToFunc(Expression e) 
		protected static Func<T> CompileTargetExpression<T>(IRezolveTarget target, IRezolverScope scope = null)
		{
			return Expression.Lambda<Func<T>>(target.CreateExpression(scope ?? Mock.Of<IRezolverScope>(),typeof(T))).Compile();
		}

		protected static Delegate CompileTargetExpression(IRezolveTarget target, IRezolverScope scope = null)
		{
			return Expression.Lambda(target.CreateExpression(scope ?? Mock.Of<IRezolverScope>())).Compile();
		}

		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolverScope scope = null)
		{
			return CompileTargetExpression<T>(target)();
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverScope scope = null)
		{
			return CompileTargetExpression(target, scope).DynamicInvoke();
		}
	}
}