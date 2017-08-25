using Rezolver.Targets;
using Rezolver.Tests.Types;
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
		public void ShouldNotAllowAnyNullConstructorArguments()
		{
            Assert.Throws<ArgumentNullException>("decoratorTarget", () => new DecoratorTarget(null, new TestTarget(), typeof(Decorated)));
            Assert.Throws<ArgumentNullException>("decoratedTarget", () => new DecoratorTarget(Target.ForType<Decorator>(), null, typeof(Decorated)));
			Assert.Throws<ArgumentNullException>("decoratedType", () => new DecoratorTarget(Target.ForType<Decorator>(), new TestTarget(), null));
		}

		[Fact]
		public void DecoratedTargetMustSupportDecoratedType()
		{
			Assert.Throws<ArgumentException>("decoratedType", () => new DecoratorTarget(Target.ForType<Decorator>(), new TestTarget(typeof(DateTime), supportsType: false), typeof(Decorated)));
		}

		[Fact]
		public void DecoratedTypeMustBeAssignableFromDecoratorType()
		{
			Assert.Throws<ArgumentException>("decoratedType", () => new DecoratorTarget(Target.ForType<DateTime>(), new TestTarget(supportsType: true), typeof(Decorated)));
		}

		[Fact]
		public void ShouldSetAllPropertiesOnConstruction()
		{
			var decoratorType = typeof(Decorator);
			var target = new TestTarget(typeof(Decorated), useFallBack: false, supportsType: true);
			var decoratedType = typeof(IDecorated);

			var result = new DecoratorTarget(Target.ForType<Decorator>(), target, decoratedType);
			Assert.NotNull(result.InnerTarget);
            Assert.Equal(typeof(Decorator), result.DeclaredType);
			Assert.Same(target, result.DecoratedTarget);
			Assert.Same(decoratedType, result.DecoratedType);
			Assert.NotNull(result.InnerTarget);
			Assert.Equal(decoratorType, result.DeclaredType);
		}
	}
}
