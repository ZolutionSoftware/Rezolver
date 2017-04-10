using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Shared.Types
{
    public class RequiresITargetCompiler
    {
        public ITargetCompiler Compiler { get; }

        public RequiresITargetCompiler(ITargetCompiler compiler)
        {
            Compiler = compiler;
        }
    }
}
