using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Specification
{
	public abstract partial class CompilerTestsBase<TCompiler, TCompileContextProvider> : CompilerTestsBase
		where TCompiler : ITargetCompiler
		where TCompileContextProvider : ICompileContextProvider
	{
		protected CompilerTestsBase(ITestOutputHelper output)
			: base(output)
		{
			
		}

		[Fact]
		public void ContainerShouldBeAbleToResolveContextProvider()
		{
			var targets = CreateTargetContainer();
			var container = CreateContainer(targets);
			Output.WriteLine("If this test fails, then all other tests in this class will likely fail");
			Assert.IsType<TCompileContextProvider>(container.Resolve<ICompileContextProvider>());
		}

		[Fact]
		public void ContainerShouldBeAbleToResolveCompiler()
		{
			var targets = CreateTargetContainer();
			var container = CreateContainer(targets);
			Output.WriteLine("If this test fails, then all other tests in this class will likely fail");
			Assert.IsType<TCompiler>(container.Resolve<ITargetCompiler>());
		}
	}
}
