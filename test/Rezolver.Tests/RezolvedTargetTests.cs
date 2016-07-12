using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class RezolvedTargetTests
  {
    [Fact]
    public void ShouldUseFallback()
    {
      var builder = new Builder();
      //have to register as object because otherwise we get a cyclic dependency
      builder.Register(new RezolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
      var container = new Container(builder);
      //the value will not be resolved, so it should fallback to 157
      var result = (int)container.Resolve<object>();
      Assert.Equal(157, result);
    }

    [Fact]
    public void ShouldValueFromOverridingContainerNotFallback()
    {
      //this test demonstrates that a ResolvedTarget defined in a root container
      //can be satisfied by an overriding container registration even when a fallback exists
      var builder = new Builder();
      builder.Register(new RezolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
      var container = new Container(builder);
      //create a CombinedContainer - which combines a new container with an existing one.
      var overridingContainer = new OverridingContainer(container);
      overridingContainer.RegisterObject(158, typeof(int));
      var result = (int)overridingContainer.Resolve<object>();
      Assert.Equal(158, result);
    }

    [Fact]
    public void ShouldUseFallbackWithOverridingContainer()
    {
      //this test makes sure that the fallback is used when an overriding container
      //is present and can't satisfy the request
      var builder = new Builder();
      builder.Register(new RezolvedTarget(typeof(int), (157).AsObjectTarget()), typeof(object));
      var container = new Container(builder);
      var overridingContainer = new OverridingContainer(container);
      var result = (int)overridingContainer.Resolve<object>();
      Assert.Equal(157, result);
    }
  }
}
