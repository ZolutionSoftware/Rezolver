using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver;
using Rezolver.Tests.Classes;

namespace Rezolver.Tests
{
	[TestClass]
	public class LazyTargetTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullInnerTarget()
		{
			IRezolveTarget target = new SingletonTarget(null);
		}
		[TestMethod]
		public void ShouldWrapObjectTarget()
		{
			IRezolveTarget target = new SingletonTarget(new ObjectTarget(1));
			Assert.AreEqual(1, target.GetObject());
		}

		[TestMethod]
		public void ShouldOnlyCreateOneInstanceFromFuncTarget()
		{
			IRezolveTarget target = new SingletonTarget(new DelegateTarget<Classes.SimpleType>(() => new Classes.SimpleType()));
			int instanceCount = Classes.SimpleType.InstanceCount;
			var i2 = target.GetObject();
			int instanceCount2 = SimpleType.InstanceCount;
			var i3 = target.GetObject();
			int instanceCount3 = SimpleType.InstanceCount;

			Assert.AreEqual(instanceCount + 1, instanceCount2);
			Assert.AreEqual(instanceCount3, instanceCount2);

		}
	}
}
