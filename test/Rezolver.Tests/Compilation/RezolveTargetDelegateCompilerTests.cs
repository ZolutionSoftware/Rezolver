using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Compilation
{
	public class TargetDelegateCompilerTests : RezolveTargetCompilerTestsBase
	{
		protected override ITargetCompiler CreateCompilerBase(string callingMethod)
		{
			return new TargetDelegateCompiler();
		}

		protected override void ReleaseCompiler(ITargetCompiler compiler)
		{
			
		}
	}
}
