using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions.Tests
{
  public class TargetDelegateCompilerTests : TargetCompilerTestsBase
  {
    protected override ITargetCompiler CreateCompilerBase(string callingMethod)
    {
      return new ExpressionCompiler();
    }

    protected override void ReleaseCompiler(ITargetCompiler compiler)
    {

    }
  }
}
