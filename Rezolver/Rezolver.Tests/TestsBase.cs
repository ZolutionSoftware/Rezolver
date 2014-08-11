using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Moq;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolver scope = null, IRezolver dynamicScope = null)
		{
			return (T)new RezolveTargetDelegateCompiler().CompileTarget(target, scope ?? CreateCompilerMock()).GetObject();
		}

		private static IRezolver CreateCompilerMock()
		{
			var scopeMock = new Mock<IRezolver>();
			scopeMock.Setup(s => s.Compiler).Returns(new RezolveTargetDelegateCompiler());

			return scopeMock.Object;
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolver scope = null, IRezolver dynamicScope = null)
		{
			return new RezolveTargetDelegateCompiler().CompileTarget(target, scope ?? CreateCompilerMock(), null, null).GetObject();
		}
	}
}