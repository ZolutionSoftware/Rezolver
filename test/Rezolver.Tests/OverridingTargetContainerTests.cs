using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class OverridingTargetContainerTests
	{
		[Fact]
		public void MustNotAllowNullParent()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var targets = new OverridingTargetContainer(null);
			});
		}

		[Fact]
		public void MustCopyParent()
		{
			var parent = new TargetContainer();
			var overriding = new OverridingTargetContainer(parent);
			Assert.Same(parent, overriding.Parent);
		}

		[Fact]
		public void ShouldInheritParentRegistration()
		{
			var parent = new TargetContainer();
			var overriding = new OverridingTargetContainer(parent);

			var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			parent.Register(parentTarget);

			Assert.Same(parentTarget, overriding.Fetch(typeof(int)));
		}

		[Fact]
		public void ShouldOverrideParentRegistration()
		{
			var parent = new TargetContainer();
			var overriding = new OverridingTargetContainer(parent);

			var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			var overrideTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			parent.Register(parentTarget);
			overriding.Register(overrideTarget);

			Assert.Same(overrideTarget, overriding.Fetch(typeof(int)));
		}

        [Fact]
        public void FetchAllShouldReturnAllTargets()
        {
            var parent = new TargetContainer();
            var overriding = new OverridingTargetContainer(parent);

            var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
            parent.Register(parentTarget);

            var overrideTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
            overriding.Register(overrideTarget);

            var fetched = overriding.FetchAll(typeof(int)).ToArray();
            Assert.Equal(2, fetched.Length);
            Assert.Same(parentTarget, fetched[0]);
            Assert.Same(overrideTarget, fetched[1]);
        }

        [Fact]
        public void EnumerableTargetShouldReturnAllItems()
        {
            var parent = new TargetContainer();
            var overriding = new OverridingTargetContainer(parent);

            var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
            parent.Register(parentTarget);

            var overrideTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
            overriding.Register(overrideTarget);

            var fetched = Assert.IsType<EnumerableTarget>(overriding.Fetch(typeof(IEnumerable<int>)));
            Assert.Equal(2, fetched.Targets.Count());
        }
    }
}
