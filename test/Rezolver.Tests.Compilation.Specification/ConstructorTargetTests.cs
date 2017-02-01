using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		//tests in here are based on similar scenarios to the Binding tests
		//for ConstructorTarget in the main test suite.

		[Fact]
		public void ShouldCompileImplicitCtor()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<NoCtor>();
			var container = CreateContainer(targets);

			var result = container.Resolve<NoCtor>();
			Assert.NotNull(result);
		}

		[Fact]
		public void ShouldCompileOneParamCtor_WithResolvedArg()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<OneCtor>();
			targets.RegisterObject(OneCtor.ExpectedValue);
			var container = CreateContainer(targets);

			var result = container.Resolve<OneCtor>();
			Assert.NotNull(result);
			Assert.Equal(OneCtor.ExpectedValue, result.Value);
		}

		[Fact]
		public void ShouldCompileTwoParamCtor_WithResolvedArgs()
		{
			Output.WriteLine("Both arguments are supplied by automatically resolving them (using RezolvedTarget)");
			var targets = CreateTargetContainer();
			targets.RegisterType<TwoCtors>();
			targets.RegisterObject(10);
			targets.RegisterObject("hello world");
			var container = CreateContainer(targets);

			var result = container.Resolve<TwoCtors>();
			Assert.NotNull(result);
			Assert.Equal(10, result.I);
			Assert.Equal("hello world", result.S);
		}
		
		[Fact]
		public void ShouldCompileConstructorSelectedByNamedArgs()
		{
			Output.WriteLine("The constructor is located JIT based on a single named argument");
			var targets = CreateTargetContainer();
			var target = ConstructorTarget.WithArgs<TwoCtors>(new { s = "hello world".AsObjectTarget() });
			targets.Register(target);

			var container = CreateContainer(targets);
			var result = container.Resolve<TwoCtors>();
			Assert.NotNull(result);
			Assert.Equal("hello world", result.S);

		}
    }
}
