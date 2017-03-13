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
		[Fact]
		public void OverridingContainer_ShouldRezolveFromParent()
		{
			//demonstrating how you can simply register directly into a container post construction
			var targets = CreateTargetContainer();
			var parentContainer = CreateContainer(targets);
			var overridingContainer = CreateOverridingContainer(parentContainer);

			int expectedInt = 10;

			targets.RegisterObject(expectedInt);

			Assert.Equal(expectedInt, overridingContainer.Resolve(typeof(int)));
		}

		public class Dependency
		{

		}

		public class Dependant
		{
			public Dependency Dependency { get; private set; }
			public Dependant(Dependency dependency)
			{
				Dependency = dependency;
			}
		}

		[Fact]
		public void OverridingContainer_ShouldRezolveDependencyFromParent()
		{
			//this is using the shortcut extension methods.
			var targets = CreateTargetContainer();
			targets.RegisterType<Dependency>();
			var parent = CreateContainer(targets);

			//the thing being that the parent container does not know how to resolve an integer 
			//and so must use the child container at call-time.
			var childTargets = CreateTargetContainer();
			childTargets.RegisterType<Dependant>();
			IContainer childContainer = CreateOverridingContainer(parent, childTargets);

			var result = childContainer.Resolve<Dependant>();
			Assert.NotNull(result);
			Assert.NotNull(result.Dependency);
		}

		[Fact]
		public void OverridingContainer_ParentShouldRezolveDependencyFromChild()
		{
			//this is using the shortcut extension methods.
			var targets = CreateTargetContainer();
			targets.RegisterType<Dependant>();
			var parent = CreateContainer(targets);

			//the thing being that the parent container does not know how to resolve the dependency
			//and so must use the child container at call-time.
			var childTargets = CreateTargetContainer();
			childTargets.RegisterType<Dependency>();
			IContainer childContainer = CreateOverridingContainer(parent, childTargets);

			var result = childContainer.Resolve<Dependant>();
			Assert.NotNull(result);
			Assert.NotNull(result.Dependency);
		}

		[Fact]
		public void OverridingContainer_ParentShouldRezolveDependencyFromChild_AfterFailing()
		{
			//this version of the test is trying to shake out any issues with compilation caches - i.e.
			//where we might cache a stub such as the MissingCompiledTarget from ContainerBase, and then
			//call it again through a child container which actually makes it work.
			var targets = CreateTargetContainer();
			targets.RegisterType<Dependant>();
			var parent = CreateContainer(targets);

			//The container interface mandates that missing resolve ops throw an InvalidOperationException
			Assert.Throws<InvalidOperationException>(() => parent.Resolve<Dependency>());

			var childTargets = CreateTargetContainer();
			childTargets.RegisterType<Dependency>();
			IContainer childContainer = CreateOverridingContainer(parent, childTargets);

			var result = childContainer.Resolve<Dependant>();
			Assert.NotNull(result);
			Assert.NotNull(result.Dependency);
		}

		[Fact]
		public void OverridingContainer_ShouldFallBackToEnumerableTargetInParent()
		{
			var targets = CreateTargetContainer();
			targets.RegisterObject(1);

			var baseContainer = CreateContainer(targets);

			var container = CreateOverridingContainer(baseContainer);
			var result = container.Resolve<IEnumerable<int>>();

			Assert.Equal(new[] { 1 }, result);
		}
	}
}
