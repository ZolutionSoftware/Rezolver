using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolverContainer scope = null, IRezolverContainer dynamicScope = null)
		{
			return (T)((ExpressionHelper.GetFactoryForTarget(scope ?? Mock.Of<IRezolverContainer>(), typeof (T), target))(dynamicScope));
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverContainer scope = null, IRezolverContainer dynamicScope = null)
		{
			return (object) (ExpressionHelper.GetFactoryForTarget(scope ?? Mock.Of<IRezolverContainer>(), null, target))(dynamicScope);
			//return CompileTargetExpression(target, scope).DynamicInvoke(null);
		}
	}
}