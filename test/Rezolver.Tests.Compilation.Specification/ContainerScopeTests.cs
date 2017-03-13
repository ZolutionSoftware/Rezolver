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
		[Fact]
		public void ContainerScope_ShouldDispose_CtorTarget()
		{
			var container = CreateContainerForSingleTarget(Target.ForType<Disposable>());

			Disposable result;
			using(var scope = container.CreateScope())
			{
				result = scope.Resolve<Disposable>();
			}

			Assert.True(result.Disposed);
		}

		[Fact]
		public void ContainerScope_ShouldDisposeAll_CtorTarget()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<Disposable>();
			targets.RegisterType<Disposable2, Disposable>();
			targets.RegisterType<Disposable3, Disposable>();
			var container = CreateContainer(targets);

			IEnumerable<Disposable> results;

			using (var scope = container.CreateScope())
			{
				results = scope.Resolve<IEnumerable<Disposable>>();
				Assert.NotNull(results);
				//don't really need to asser this because IEnumerable tests do it.
				Assert.Equal(3, results.Count());
			}

			Assert.All(results, r => Assert.True(r.Disposed));
		}

		[Fact]
		public void ContainerScope_ShouldCreateOneExplicit_CtorTarget()
		{
			var targets = CreateTargetContainer();
			targets.RegisterScoped<Disposable>();
			var container = CreateContainer(targets);

			Disposable expected;

			using (var scope = container.CreateScope())
			{
				expected = scope.Resolve<Disposable>();
				var second = scope.Resolve<Disposable>();
				Assert.Same(expected, second);
			}
			Assert.True(expected.Disposed);
		}

		[Fact]
		public void ContainerScope_ShouldDisposeDependencyDisposables()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<RequiresThreeDisposables>();
			targets.RegisterAll(Target.ForType<Disposable>(),
				Target.ForType<Disposable2>(),
				Target.ForType<Disposable3>());

			RequiresThreeDisposables result = null;
			using (var container = CreateScopedContainer(targets))
			{
				result = container.Resolve<RequiresThreeDisposables>();
			}

			Assert.True(result.First.Disposed);
			Assert.True(result.Second.Disposed);
			Assert.True(result.Third.Disposed);
		}

		[Fact]
		public void ContainerScope_ShouldDisposeDependenciesInChildScope()
		{
			var targets = CreateTargetContainer();
			targets.RegisterType<RequiresThreeDisposables>();
			targets.RegisterAll(Target.ForType<Disposable>(),
				Target.ForType<Disposable2>(),
				Target.ForType<Disposable3>());

			RequiresThreeDisposables result = null;
			using (var container = CreateScopedContainer(targets))
			{
				using (var childScope = container.CreateScope())
				{
					result = childScope.Resolve<RequiresThreeDisposables>();
				}

				Assert.True(result.First.Disposed);
				Assert.True(result.Second.Disposed);
				Assert.True(result.Third.Disposed);
			}
		}

		[Fact]
		public void ContainerScope_SingletonShouldOnlyBeDisposedByRoot()
		{
			var targets = CreateTargetContainer();
			targets.RegisterSingleton<Disposable>();

			Disposable disposable = null;

			using (var container = CreateScopedContainer(targets))
			{
				using(var childScope = container.CreateScope())
				{
					disposable = childScope.Resolve<Disposable>();
				}
				Assert.False(disposable.Disposed);
			}

			Assert.True(disposable.Disposed);
		}
    }
}
