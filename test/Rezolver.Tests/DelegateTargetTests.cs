using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DelegateTargetTests : TestsBase
	{
		[Fact]
		public void ShouldNotAllowNullDelegate()
		{
			//notice here that delegate targets MUST have a target type, because they are generic.
			//we use generic delegates because otherwise we can't guarantee what the underlying type
			//is that's being returned - which we need for IRezolveTarget.

			Assert.Throws<ArgumentNullException>(() => new DelegateTarget<object>(null));
		}

		[Fact]
		public void ShouldSupportAndReturnInt()
		{
			ITarget target = new DelegateTarget<int>(() => 1);
			Assert.True(target.SupportsType(typeof(int)));
			Assert.Equal(1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldSupportAndReturnObject()
		{
			ITarget target = new DelegateTarget<object>(() => 1);
			Assert.True(target.SupportsType(typeof(object)));
			Assert.Equal(1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldCreateNewInstanceEachCall()
		{
			using (var session = InstanceCountingType.NewSession())
			{
				ITarget target = new DelegateTarget<InstanceCountingType>(() => new InstanceCountingType());
				int currentInstances = InstanceCountingType.InstanceCount;
				var result = GetValueFromTarget(target);
				Assert.Equal(currentInstances + 1, InstanceCountingType.InstanceCount);
				var result2 = GetValueFromTarget(target);
				Assert.Equal(currentInstances + 2, InstanceCountingType.InstanceCount);
			}
		}
	}
}
