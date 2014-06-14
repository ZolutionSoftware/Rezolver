using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Tests.Classes;

namespace Rezolver.Tests
{
	[TestClass]
	public class DelegateTargetTests : TestsBase
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullDelegate()
		{
			//notice here that delegate targets MUST have a target type, because they are generic.
			//we use generic delegates because otherwise we can't guarantee what the underlying type
			//is that's being returned - which we need for IRezolveTarget.

			IRezolveTarget target = new DelegateTarget<object>(null);
		}

		[TestMethod]
		public void ShouldSupportAndReturnInt()
		{
			IRezolveTarget target = new DelegateTarget<int>(() => 1);
			Assert.IsTrue(target.SupportsType(typeof(int)));
			Assert.AreEqual(1, GetValueFromTarget(target));
		}

		[TestMethod]
		public void ShouldSupportAndReturnObject()
		{
			IRezolveTarget target = new DelegateTarget<object>(() => 1);
			Assert.IsTrue(target.SupportsType(typeof(object)));
			Assert.AreEqual(1, GetValueFromTarget(target));
		}

		[TestMethod]
		public void ShouldCreateNewInstanceEachCall()
		{
			IRezolveTarget target = new DelegateTarget<SimpleType>(() => new SimpleType());
			int currentInstances = SimpleType.InstanceCount;
			var result = GetValueFromTarget(target);
			Assert.AreEqual(currentInstances + 1, SimpleType.InstanceCount);
			var result2 = GetValueFromTarget(target);
			Assert.AreEqual(currentInstances + 2, SimpleType.InstanceCount);
		}


	}
}
