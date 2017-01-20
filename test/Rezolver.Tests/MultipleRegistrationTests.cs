using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rezolver.Tests
{
  /// <summary>
  /// tests rezolverbuilder and container types for whether they support IEnumerable{Service} - both after registering
  /// a single entry, or multiple.
  /// </summary>
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

    [Fact]
    public void ShouldResolveOneServiceForIEnumerableDependency()
    {
      var container = CreateContainer();
      container.Register((10).AsObjectTarget());
      container.RegisterType<RequiresServices>();
      container.RegisterType<ServiceA, IService>();
      var result = container.Resolve<RequiresServices>();
      Assert.Single<IService>(result.Services);
      Assert.IsType<ServiceA>(result.Services.First());
    }

    [Fact]
    public void ShouldRegisterAndResolveMultipleServiceInstances()
    {
      var container = CreateContainer();
      container.Register((10).AsObjectTarget());
      container.Register((20.0).AsObjectTarget());
      container.Register("hello multiple".AsObjectTarget());
      container.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));

      var result = container.Resolve(typeof(IEnumerable<IService>));
      Assert.NotNull(result);
      var resultArray = ((IEnumerable<IService>)result).ToArray();
      Assert.Equal(2, resultArray.Length);
    }

    [Fact]
    public void ShouldRegisterAndResolveMultipleServiceInstancesAsDependency()
    {
      var container = CreateContainer();
      container.Register((10).AsObjectTarget());
      container.Register((20.0).AsObjectTarget());
      container.Register("hello multiple".AsObjectTarget());
      container.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));
      container.Register(ConstructorTarget.Auto<RequiresServices>());

      var result = (RequiresServices)container.Resolve(typeof(RequiresServices));
      Assert.NotNull(result.Services);
      Assert.Equal(2, result.Services.Count());
      Assert.Equal(1, result.Services.OfType<ServiceA>().Count());
      Assert.Equal(1, result.Services.OfType<ServiceB>().Count());
    }
  }
}
