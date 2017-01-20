using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Targets
{
    public class ChangeTypeTargetTests
    {
		protected class ValidTypeForTestTarget : TestTarget {
			ValidTypeForTestTarget() : base(typeof(TestTarget)) { }
		}

		protected class InvalidTypeForTestTarget { }

		private TestTarget CreateTestTarget()
		{
			return new TestTarget(typeof(TestTarget));
		}

		[Fact]
		public void ShouldConstructChangeTypeTarget()
		{
			var innerTarget = CreateTestTarget();

			var target = new ChangeTypeTarget(innerTarget, typeof(ValidTypeForTestTarget));

			Assert.Same(innerTarget, target.InnerTarget);
		}

		[Fact]
		public void ShouldFailIfInnerTargetNull()
		{
			Assert.Throws<ArgumentNullException>("innerTarget", () => new ChangeTypeTarget(null, typeof(object)));
		}

		[Fact]
		public void ShouldFailIfTargetTypeNull()
		{
			Assert.Throws<ArgumentNullException>("targetType", () => new ChangeTypeTarget(CreateTestTarget(), null));
		}

		[Fact]
		public void ShouldFailIfTypesArentCompatible()
		{
			var innerTarget = CreateTestTarget();

			Assert.Throws<ArgumentException>("targetType", () => new ChangeTypeTarget(innerTarget, typeof(InvalidTypeForTestTarget)));
		}
    }
}
