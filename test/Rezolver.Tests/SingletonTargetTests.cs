using Rezolver.Targets;
using Rezolver.Tests.TestTypes;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class SingletonTargetTests : TestsBase
	{
		[Fact]
		public void ShouldNotAllowNullInnerTarget()
		{
			Assert.Throws<ArgumentNullException>(() => new SingletonTarget(null));
		}
		[Fact]
		public void ShouldWrapObjectTarget()
		{
			ITarget target = new SingletonTarget(new ObjectTarget(1));
			Assert.Equal(1, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldOnlyCreateOneInstanceFromFuncTarget()
		{
			using (var session = InstanceCountingType.NewSession())
			{
				ITarget target = new SingletonTarget(new DelegateTarget<InstanceCountingType>(() => new InstanceCountingType()));
				int instanceCount = InstanceCountingType.InstanceCount;
				var i2 = GetValueFromTarget(target);
				int instanceCount2 = InstanceCountingType.InstanceCount;
				var i3 = GetValueFromTarget(target);
				int instanceCount3 = InstanceCountingType.InstanceCount;

				Assert.Equal(instanceCount + 1, instanceCount2);
				Assert.Equal(instanceCount3, instanceCount2);
			}
		}

		public class TestGenericType<TArg1>
		{

		}

		[Fact]
		public void SingletonShouldCreateInstanceForEachUniqueType()
		{
			//a bug that occurred when interfacing with Asp.Net MVC6 - they use a singleton Generic for IOptions`1, and the code was 
			//only ever creating the one instance of any generic, so 2nd and subsequent calls would get the instance that was first
			//created, regardless of whether the generic types were the same!
			var target = GenericConstructorTarget.Auto(typeof(TestGenericType<>)).Singleton();
			var r = new Container();
			r.Register(target);

			var t = r.Resolve(typeof(TestGenericType<string>));
			//Causes InvalidCastException in unfixed code.
			var t2 = r.Resolve(typeof(TestGenericType<int>));
		}
	}
}
