using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class MultipleRegistrationTests : TestsBase
	{
		public interface IService
		{

		}

		public class ServiceA : IService
		{
			public int IntValue { get; private set; }
			public ServiceA(int intValue)
			{
				IntValue = intValue;
			}
		}

		public class ServiceB : IService
		{
			public double DoubleValue { get; private set; }
			public string StringValue { get; private set; }
		}

		public class RequiresServices
		{
			public IEnumerable<IService> Services { get; private set; }
			public RequiresServices(IEnumerable<IService> services)
			{
				Services = services;
			}
		}

		[TestMethod]
		public void ShouldRegisterAndResolveMultipleServiceInstances()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((10).AsObjectTarget());
			rezolver.Register((20.0).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));

			var result = rezolver.Resolve(typeof(IEnumerable<IService>));
			Assert.IsInstanceOfType(result, typeof(IEnumerable<IService>));
			var resultArray = ((IEnumerable<IService>)result).ToArray();
			Assert.AreEqual(2, resultArray.Length);
		}

		[TestMethod]
		public void ShouldRegisterAndResolveMultipleServiceInstancesAsDependency()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((10).AsObjectTarget());
			rezolver.Register((20.0).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));
			rezolver.Register(ConstructorTarget.Auto<RequiresServices>());

			var result = (RequiresServices)rezolver.Resolve(typeof(RequiresServices));
			Assert.IsNotNull(result.Services);
			Assert.AreEqual(2, result.Services.Count());
			Assert.AreEqual(1, result.Services.OfType<ServiceA>().Count());
			Assert.AreEqual(1, result.Services.OfType<ServiceB>().Count());
		}
	}
}
