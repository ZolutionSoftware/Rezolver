using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Holds a reference to the default compiler for this application.
	/// 
	/// Unless you explicitly pass compilers to your containers, you will need to 
	/// set the <see cref="Default"/> property of this class to the compiler that
	/// you wish to use by default.
	/// </summary>
	public static class RezolveTargetCompiler
	{
		private static IRezolveTargetCompiler _default;

		private class StubCompiler : IRezolveTargetCompiler
		{
			public static readonly StubCompiler Instance = new StubCompiler();

			public ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolver scope,
				ParameterExpression dynamicRezolverExpression, Stack<IRezolveTarget> targetStack)
			{
				throw new NotImplementedException("You must set the RezolveTargetCompiler.Default to a non-null reference to a compiler if you intend to use the system default rezolver compiler.");
			}
		}

		public static IRezolveTargetCompiler Default
		{
			get { return _default ?? StubCompiler.Instance; }
			set { _default = value; }
		}
	}
}