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
			var container = CreateContainerForSingleTarget(ConstructorTarget.Auto<Disposable>());

			Disposable result;
			//will change the API to use a member
			using(var scope = new ContainerScope(container))
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

			using (var scope = new ContainerScope(container))
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

			using (var scope = new ContainerScope(container))
			{
				expected = scope.Resolve<Disposable>();
				var second = scope.Resolve<Disposable>();
				Assert.Same(expected, second);
			}
			Assert.True(expected.Disposed);
		}
    }
}
