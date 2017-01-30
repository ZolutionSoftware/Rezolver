using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Expressions
{
    public class ExpressionCompilerSpecificationTests
		: Rezolver.Tests.Compilation.Specification.CompilerTestsBase<ExpressionCompiler, ExpressionCompiler>
    {
		public ExpressionCompilerSpecificationTests(ITestOutputHelper output)
			: base(output)
		{

		}

		protected override ITargetContainer CreateTargetContainer([CallerMemberName] string testName = null)
		{
			var toReturn = new TargetContainer();
			toReturn.UseExpressionCompiler();
			return toReturn;
		}
	}
}
