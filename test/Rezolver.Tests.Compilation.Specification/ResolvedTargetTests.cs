using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		//the ResolvedTarget is heavily tested via many of the other tests because of
		//Rezolver's reliance on implicit resolving of dependencies.
		//this suite targets the most direct scenarios.

		[Fact]
		public void ResolvedTarget_ShouldUseFallback()
		{
			var targets = CreateTargetContainer();
			//have to register as object because otherwise we get a cyclic dependency
			targets.Register(new ResolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
			var container = CreateContainer(targets);
			//the value will not be resolved, so it should fallback to 157
			var result = (int)container.Resolve<object>();
			Assert.Equal(157, result);
		}

		[Fact]
		public void ResolvedTarget_ShouldValueFromOverridingContainerNotFallback()
		{
			//this test demonstrates that a ResolvedTarget defined in a root container
			//can be satisfied by an overriding container registration even when a fallback exists

			var targets = CreateTargetContainer();
			targets.Register(new ResolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
			var container = CreateContainer(targets);

			//create targets for our overriding container
			var childTargets = CreateTargetContainer();
			childTargets.RegisterObject(158, typeof(int));
			var overridingContainer = CreateOverridingContainer(container, childTargets);
						
			var result = (int)overridingContainer.Resolve<object>();
			Assert.Equal(158, result);
		}

		[Fact]
		public void ResolvedTarget_ShouldUseFallbackWithOverridingContainer()
		{
			//this test makes sure that the fallback is used when an overriding container
			//is present and doesn't have an override for the type
			var targets = CreateTargetContainer();
			targets.Register(new ResolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
			var container = CreateContainer(targets);

			var overridingContainer = CreateOverridingContainer(container);
			var result = (int)overridingContainer.Resolve<object>();
			Assert.Equal(157, result);
		}
	}
}
