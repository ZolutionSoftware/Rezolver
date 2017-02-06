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

		protected override IContainer CreateContainer(ITargetContainer targets, [CallerMemberName] string testName = null)
		{
			//in reality, most applications are going to use the UseExpressionCompiler.Configure() static method
			return new Container(targets, ExpressionCompiler.ConfigProvider);
		}
	}
}
