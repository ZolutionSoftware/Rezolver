using Rezolver.Compilation;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Specification
{
	public abstract partial class CompilerTestsBase<TCompiler> : CompilerTestsBase
		where TCompiler : ITargetCompiler
	{
		protected CompilerTestsBase(ITestOutputHelper output)
			: base(output)
		{
			
		}

		[Fact]
		public void Configured_Container_ShouldGetCompilerAsOption()
		{
			var targets = CreateTargetContainer();
			var container = CreateContainer(targets);
			Output.WriteLine("If this test fails, then all other tests in this class will likely fail");
			Assert.IsType<TCompiler>(targets.GetOption<ITargetCompiler>());
		}

        [Fact]
        public void Configured_OverridingContainer_ShouldAlsoGetCompilerAsOption()
        {
            Output.WriteLine("Testing that the container returned from CreateOverridingContainer can also get a compiler from options as the base container does.  If this fails, then any tests to do with overriding containers will fail.");
            var container = CreateContainer(CreateTargetContainer());
            var overrideContainer = CreateOverridingContainer(container);

            Assert.IsType<TCompiler>(overrideContainer.GetOption<ITargetCompiler>());
        }
	}
}
