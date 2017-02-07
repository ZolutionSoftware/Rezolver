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
		//for ConstructorTarget in the main test suite - except there are fewer, because
		//there are actually very few unique cases for a compiler to deal with when it comes
		//to executing constructors.

		//Most of the complexity comes from things like RezolvedTargets etc.

		[Fact]
		public void ConstructorTarget_ImplicitCtor()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<NoCtor>();
			var container = CreateContainer(targets);

			var result = container.Resolve<NoCtor>();
			Assert.NotNull(result);
		}

		[Fact]
		public void ConstructorTarget_OneParamCtor_WithResolvedArg()
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
		public void ConstructorTarget_TwoParamCtor_WithResolvedArgs()
		{
			Output.WriteLine("Both arguments are supplied by automatically resolving them (using ResolvedTarget)");
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
		public void ConstructorTarget_CtorSelectedByNamedArgs()
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
