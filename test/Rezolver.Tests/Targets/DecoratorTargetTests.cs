using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
    public class DecoratorTargetTests : TargetTestsBase
    {
		public DecoratorTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowAnyNullConstructoArguments()
		{
			Assert.Throws<ArgumentNullException>("decoratorType", () => new DecoratorTarget(null, new TestTarget(), typeof(Decorated)));
			Assert.Throws<ArgumentNullException>("decoratedTarget", () => new DecoratorTarget(typeof(Decorator), null, typeof(Decorated)));
			Assert.Throws<ArgumentNullException>("decoratedType", () => new DecoratorTarget(typeof(Decorator), new TestTarget(), null));
		}

		[Fact]
		public void DecoratedTargetMustSupportDecoratedType()
		{
			Assert.Throws<ArgumentException>("decoratedType", () => new DecoratorTarget(typeof(Decorator), new TestTarget(typeof(DateTime), supportsType: false), typeof(Decorated)));
		}

		[Fact]
		public void DecoratedTypeMustBeAssignableFromDecoratorType()
		{
			Assert.Throws<ArgumentException>("decoratedType", () => new DecoratorTarget(typeof(DateTime), new TestTarget(supportsType: false), typeof(Decorated)));
		}

		[Fact]
		public void AllPropertiesSetOnConstruction()
		{
			var decoratorType = typeof(Decorator);
			var target = new TestTarget(typeof(Decorated), useFallBack: false, supportsType: true);
			var decoratedType = typeof(Decorated);

			var result = new DecoratorTarget(decoratorType, target, decoratedType);
			Assert.Same(decoratorType, result.DecoratorType);
			Assert.Same(target, result.DecoratedTarget);
			Assert.Same(decoratedType, result.DecoratedType);
			Assert.NotNull(result.Target);
			Assert.Equal(decoratorType, result.DeclaredType);
		}
	}
}
