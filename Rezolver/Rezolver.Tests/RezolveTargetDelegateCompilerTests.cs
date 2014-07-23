using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolveTargetDelegateCompilerTests : RezolveTargetCompilerTestsBase
	{
		protected override IRezolveTargetCompiler CreateCompilerBase()
		{
			return new RezolveTargetDelegateCompiler();
		}

		protected override void ReleaseCompiler(IRezolveTargetCompiler compiler)
		{

		}
	}



	public class SpecialRezolveTargetCompiler : IRezolveTargetCompiler
	{
		public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack)
		{
			throw new NotImplementedException();
		}
	}
}
