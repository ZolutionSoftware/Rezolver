using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class OverridingContainerTests : TestsBase
  {
    [Fact]
    public void ShouldRezolveFromBaseRezolver()
    {
      //demonstrating how you can simply register directly into a container post construction
      var rezolver1 = new Container(compiler: new TargetDelegateCompiler());
      var rezolver2 = new OverridingContainer(rezolver1);

      int expectedInt = 10;

      rezolver1.Register(expectedInt.AsObjectTarget());


      Assert.Equal(expectedInt, rezolver2.Resolve(typeof(int)));
    }

    private class TypeWithConstructorArg
    {
      public int Value { get; private set; }

      public TypeWithConstructorArg(int value)
      {
        Value = value;
      }
    }

    [Fact]
    public void ShouldRezolveIntDependencyFromBaseRezolver()
    {
      //this is using constructorTarget with a prescribed new expression
      var container = CreateADefaultRezolver();
      container.RegisterType<TypeWithConstructorArg>();

      //the thing being that the underlying Builder does not know how too resolve an integer without
      //being passed a dynamic container at call-time.
      //this mocks a dynamically defined container that an application creates in response to transient information only
      int expected = -1;
      OverridingContainer dynamicRezolver = new OverridingContainer(container);
      dynamicRezolver.RegisterObject(expected);

      var result = (TypeWithConstructorArg)dynamicRezolver.Resolve(typeof(TypeWithConstructorArg));
      Assert.NotNull(result);
      Assert.Equal(expected, result.Value);
    }

    public class Bug_Dependency
    {

    }

    public class Bug_Dependant
    {
      public Bug_Dependency Dependency { get; private set; }
      public Bug_Dependant(Bug_Dependency dependency)
      {
        Dependency = dependency;
      }
    }

    [Fact]
    public void Bug_DynamicRezolverFallingBackToDefaultOnConstructorParameter()
    {
      var rezolver1 = new Container();
      var rezolver2 = new OverridingContainer(rezolver1);

      rezolver1.Register(ConstructorTarget.Auto<Bug_Dependant>());
      rezolver2.Register(ConstructorTarget.Auto<Bug_Dependency>());

      var result = rezolver2.Resolve(typeof(Bug_Dependant));
      Assert.NotNull(result);
      Assert.IsType<Bug_Dependant>(result);
      Assert.NotNull(((Bug_Dependant)result).Dependency);
    }

    [Fact]
    public void ShouldFallBackToEnumerableTargetInBase()
    {
      //ISSUE 
      var baseResolver = new Container();
      baseResolver.RegisterObject(1);

      var combinedResolver = new OverridingContainer(baseResolver);
      var result = combinedResolver.Resolve<IEnumerable<int>>();

      Assert.NotNull(result);
      Assert.NotEmpty(result);
    }
  }
}
