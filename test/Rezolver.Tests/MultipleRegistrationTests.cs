using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rezolver.Tests
{
  /// <summary>
  /// tests rezolverbuilder and rezolver types for whether they support IEnumerable{Service} - both after registering
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
      var rezolver = CreateADefaultRezolver();
      rezolver.Register((10).AsObjectTarget());
      rezolver.RegisterType<RequiresServices>();
      rezolver.RegisterType<ServiceA, IService>();
      var result = rezolver.Resolve<RequiresServices>();
      Assert.Single<IService>(result.Services);
      Assert.IsType<ServiceA>(result.Services.First());
    }

    [Fact]
    public void ShouldRegisterAndResolveMultipleServiceInstances()
    {
      var rezolver = CreateADefaultRezolver();
      rezolver.Register((10).AsObjectTarget());
      rezolver.Register((20.0).AsObjectTarget());
      rezolver.Register("hello multiple".AsObjectTarget());
      rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));

      var result = rezolver.Resolve(typeof(IEnumerable<IService>));
      Assert.NotNull(result);
      var resultArray = ((IEnumerable<IService>)result).ToArray();
      Assert.Equal(2, resultArray.Length);
    }

    [Fact]
    public void ShouldRegisterAndResolveMultipleServiceInstancesAsDependency()
    {
      var rezolver = CreateADefaultRezolver();
      rezolver.Register((10).AsObjectTarget());
      rezolver.Register((20.0).AsObjectTarget());
      rezolver.Register("hello multiple".AsObjectTarget());
      rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));
      rezolver.Register(ConstructorTarget.Auto<RequiresServices>());

      var result = (RequiresServices)rezolver.Resolve(typeof(RequiresServices));
      Assert.NotNull(result.Services);
      Assert.Equal(2, result.Services.Count());
      Assert.Equal(1, result.Services.OfType<ServiceA>().Count());
      Assert.Equal(1, result.Services.OfType<ServiceB>().Count());
    }
  }
}
