using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolverContainer scope = null)
		{
			return (T)((ExpressionHelper.GetFactoryForTarget(scope ?? Mock.Of<IRezolverContainer>(), typeof (T), target))(null));
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverContainer scope = null)
		{
			return (object) (ExpressionHelper.GetFactoryForTarget(scope ?? Mock.Of<IRezolverContainer>(), null, target));
			//return CompileTargetExpression(target, scope).DynamicInvoke(null);
		}
	}
}