﻿using System;
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
				var builder = new OverridingTargetContainer(null);
			});
		}

		[Fact]
		public void MustCopyParent()
		{
			var parent = new TargetContainer();
			var childBuilder = new OverridingTargetContainer(parent);
			Assert.Same(parent, childBuilder.Parent);
		}

		[Fact]
		public void ShouldInheritParentRegistration()
		{
			var parent = new TargetContainer();
			var child = new OverridingTargetContainer(parent);

			var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			parent.Register(parentTarget);

			Assert.Same(parentTarget, child.Fetch(typeof(int)));
		}

		[Fact]
		public void ShouldOverrideParentRegistration()
		{
			var parent = new TargetContainer();
			var child = new OverridingTargetContainer(parent);

			var parentTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			var childTarget = new TestTarget(typeof(int), useFallBack: false, supportsType: true);
			parent.Register(parentTarget);
			child.Register(childTarget);

			Assert.Same(childTarget, child.Fetch(typeof(int)));
		}
	}
}