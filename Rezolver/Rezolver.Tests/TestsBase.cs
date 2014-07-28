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
			return (T)new RezolveTargetDelegateCompiler().CompileTarget(target, scope ?? CreateCompilerMock()).GetObject();
		}

		private static IRezolverContainer CreateCompilerMock()
		{
			var scopeMock = new Mock<IRezolverContainer>();
			scopeMock.Setup(s => s.Compiler).Returns(new RezolveTargetDelegateCompiler());

			return scopeMock.Object;
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolverContainer scope = null, IRezolverContainer dynamicScope = null)
		{
			return new RezolveTargetDelegateCompiler().CompileTarget(target, scope ?? CreateCompilerMock(), null, null).GetObject();
		}
	}
}