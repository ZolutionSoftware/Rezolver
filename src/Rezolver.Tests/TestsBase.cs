using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolver rezolver = null)
		{
			var compiledTarget = new RezolveTargetDelegateCompiler().CompileTarget(target, new CompileContext(rezolver ?? CreateRezolverMock(), typeof(T)));
			if (rezolver == null)
				return (T)compiledTarget.GetObject(new RezolveContext(CreateRezolverMock(), typeof(T)));
			else
				return (T)compiledTarget.GetObject(new RezolveContext(rezolver, typeof(T)));
		}

		private static IRezolver CreateRezolverMock()
		{
			var scopeMock = new Mock<IRezolver>();
			scopeMock.Setup(s => s.Compiler).Returns(new RezolveTargetDelegateCompiler());

			return scopeMock.Object;
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolver rezolver = null, Type targetType = null)
		{
			return new RezolveTargetDelegateCompiler().CompileTarget(target, 
				new CompileContext(rezolver ?? CreateRezolverMock())).GetObject(new RezolveContext(CreateRezolverMock(), targetType));
		}

		protected static DefaultRezolver CreateADefaultRezolver()
		{
			return new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
		}
	}
}