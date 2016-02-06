using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Compilation
{
	public class RezolveTargetDelegateCompilerTests : RezolveTargetCompilerTestsBase
	{
		protected override IRezolveTargetCompiler CreateCompilerBase(string callingMethod)
		{
			return new RezolveTargetDelegateCompiler();
		}

		protected override void ReleaseCompiler(IRezolveTargetCompiler compiler)
		{
			
		}
	}
}
