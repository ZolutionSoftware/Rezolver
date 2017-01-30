using Rezolver.Targets;
using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using OldTypes = Rezolver.Tests.TestTypes;

namespace Rezolver.Tests
{
  public class GenericConstructorTargetTests : TestsBase
  {
    [Fact]
    public void ShouldCreateGenericNoCtorClass()
    {
      ITarget t = GenericConstructorTarget.Auto(typeof(GenericNoCtor<>));
      Assert.NotNull(t);
      Assert.Equal(typeof(GenericNoCtor<>), t.DeclaredType);
      //try and build an instance 
      var instance = GetValueFromTarget<GenericNoCtor<int>>(t);
      Assert.NotNull(instance);
      Assert.Equal(default(int), instance.Value);
    }

    [Fact]
    public void ShouldResolveAGenericNoCtorClass()
    {
      //similar test to above, but we're testing that it works when you put the target inside the
      //default resolver.
      var container = new Container();
      container.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));
      var instance = (GenericNoCtor<int>)container.Resolve(typeof(GenericNoCtor<int>));
      Assert.NotNull(instance);
    }

    [Fact]
    public void ShouldCreateAGenericClass()
    {
      //typical pattern already 
      var container = CreateContainer();
      container.RegisterObject(1);
      ITarget t = GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>));
      Assert.NotNull(t);
      Assert.Equal(typeof(OldTypes.Generic<>), t.DeclaredType);
      var instance = GetValueFromTarget<OldTypes.Generic<int>>(t, container);
      Assert.NotNull(instance);
      Assert.Equal(1, instance.Value);
    }

    [Fact]
    public void ShouldRezolveAGenericClass()
    {
      //in this one, using DefaultRezolver, we're going to test a few parameter types
      var container = new Container();
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)));
      container.Register((2).AsObjectTarget());
      container.Register((3).AsObjectTarget(typeof(int?)));
      container.Register("hello world".AsObjectTarget());
      var instance1 = (OldTypes.Generic<int>)container.Resolve(typeof(OldTypes.Generic<int>));
      var instance2 = (OldTypes.Generic<string>)container.Resolve(typeof(OldTypes.Generic<string>));
      var instance3 = (OldTypes.Generic<int?>)container.Resolve(typeof(OldTypes.Generic<int?>));

      Assert.Equal(2, instance1.Value);
      Assert.Equal("hello world", instance2.Value);
      Assert.Equal(3, instance3.Value);
    }

    //now test that the target should work when used as the target of a dependency look up
    //just going to use the DefaultRezolver for this as it's far easier to setup the test.

    [Fact]
    public void ShouldRezolveAClosedGenericDependency()
    {
      var container = new Container();
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)));
      container.Register((2).AsObjectTarget());
      container.Register(ConstructorTarget.Auto<HasGenericDependency>());

      var result = (HasGenericDependency)container.Resolve(typeof(HasGenericDependency));
      Assert.NotNull(result);
      Assert.NotNull(result.Dependency);
      Assert.Equal(2, result.Dependency.Value);
    }

    [Fact]
    public void ShouldRezolveNestedGenericDependency()
    {
      //this one is more complicated.  Passing a closed generic as a type argument to another
      //generic.
      //note that this isn't the most complicated it can get, however: that would be using
      //the type argument as a type argument to another open generic dependency.  That one is on it's way.
      var container = new Container();

      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)));
      container.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));

      var result = (OldTypes.Generic<GenericNoCtor<int>>)container.Resolve(typeof(OldTypes.Generic<GenericNoCtor<int>>));
      Assert.NotNull(result);
      Assert.NotNull(result.Value);
      Assert.Equal(0, result.Value.Value);
    }


    //this one is the open generic nested dependency check

    [Fact]
    public void ShouldResolveNestedeOpenGenericDependency()
    {
      var container = new Container();

      container.Register((10).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)));
      container.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericDependency<>)));

      var result = (HasOpenGenericDependency<int>)container.Resolve(typeof(HasOpenGenericDependency<int>));
      Assert.NotNull(result);
      Assert.NotNull(result.Dependency);
      Assert.Equal(10, result.Dependency.Value);
    }

    //now moving on to rezolving interface instead of the type directly

    [Fact]
    public void ShouldResolveGenericViaInterface()
    {
      var container = new Container();
      container.Register((20).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)), typeof(OldTypes.IGeneric<>));

      var result = (OldTypes.IGeneric<int>)container.Resolve(typeof(OldTypes.IGeneric<int>));
      Assert.IsType<OldTypes.Generic<int>>(result);
      Assert.Equal(20, result.Value);
    }

    [Fact]
    public void ShouldRezolveGenericViaGenericInterface()
    {
      //first version of this test - where the nested generic interface is different
      //to the outer generic interface.  At the time of writing, making it the same causes
      //a circular dependency - see Bug #7

      var container = CreateContainer();
      //we need three dependencies registered - the inner T, an IGenericA<> and 
      //an IGeneric<IGenericA<T>>.
      container.Register((25).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)), typeof(OldTypes.IGeneric<>));
      //note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
      //supply an open generic as a type parameter to a generic is not valid.
      container.Register(GenericConstructorTarget.Auto(typeof(GenericGeneric<>)), typeof(OldTypes.IGeneric<>).MakeGenericType(typeof(OldTypes.IGeneric<>)));

      var result = (OldTypes.IGeneric<OldTypes.IGeneric<int>>)container.Resolve(typeof(OldTypes.IGeneric<OldTypes.IGeneric<int>>));

      Assert.Equal(25, result.Value.Value);
      Assert.IsType<GenericGeneric<int>>(result);
      Assert.IsType<OldTypes.Generic<int>>(result.Value);
    }

    [Fact]
    public void ShouldResolveClosedGenericViaInterfaceDependency()
    {
      var container = new Container();
      container.Register((30).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)), typeof(OldTypes.IGeneric<>));
      container.Register(ConstructorTarget.Auto<HasGenericInterfaceDependency>());
      var result = (HasGenericInterfaceDependency)container.Resolve(typeof(HasGenericInterfaceDependency));
      Assert.NotNull(result.Dependency);
      Assert.Equal(30, result.Dependency.Value);
    }

    [Fact]
    public void ShouldResolveOpenGenericViaInterfaceDependency()
    {
      var container = CreateContainer();
      container.Register((40).AsObjectTarget());
      container.Register((50d).AsObjectTarget(typeof(double?))); //will that work?
      container.Register("hello interface generics!".AsObjectTarget());
      //now register the IGeneric<T>
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic<>)), typeof(OldTypes.IGeneric<>));
      container.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericInterfaceDependency<>)));
      var resultWithInt = (HasOpenGenericInterfaceDependency<int>)container.Resolve(typeof(HasOpenGenericInterfaceDependency<int>));
      var resultWithNullableDouble = (HasOpenGenericInterfaceDependency<double?>)container.Resolve(typeof(HasOpenGenericInterfaceDependency<double?>));
      var resultWithString = (HasOpenGenericInterfaceDependency<string>)container.Resolve(typeof(HasOpenGenericInterfaceDependency<string>));
      Assert.NotNull(resultWithInt);
      Assert.NotNull(resultWithNullableDouble);
      Assert.NotNull(resultWithString);
      Assert.NotNull(resultWithInt.Dependency);
      Assert.NotNull(resultWithNullableDouble.Dependency);
      Assert.NotNull(resultWithString.Dependency);
      Assert.Equal(40, resultWithInt.Dependency.Value);
      Assert.Equal(50, resultWithNullableDouble.Dependency.Value);
      Assert.Equal("hello interface generics!", resultWithString.Dependency.Value);
    }

    //now on to the multiple type parameters
    //first by direct type
    [Fact]
    public void ShouldResolveGenericTypeWith2Parameters()
    {
      var container = CreateContainer();
      container.Register((60).AsObjectTarget());
      container.Register("hello multiple".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic2<,>)));
      var result = (OldTypes.Generic2<int, string>)container.Resolve(typeof(OldTypes.Generic2<int, string>));
      Assert.Equal(60, result.Value1);
      Assert.Equal("hello multiple", result.Value2);

      var result2 = (OldTypes.Generic2<string, int>)container.Resolve(typeof(OldTypes.Generic2<string, int>));
      Assert.Equal("hello multiple", result2.Value1);
      Assert.Equal(60, result2.Value2);
    }

    //TODO: resolve by base (DerivedGeneric<T>) and then probably using reversed parameters also.
    [Fact]
    public void ShouldResolveGenericTypeWith2ParametersByInterface()
    {
      var container = CreateContainer();
      container.Register((70).AsObjectTarget());
      container.Register("hello multiple interface".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(OldTypes.Generic2<,>)), typeof(OldTypes.IGeneric2<,>));
      var result = (OldTypes.IGeneric2<int, string>)container.Resolve(typeof(OldTypes.IGeneric2<int, string>));
      Assert.Equal(70, result.Value1);
      Assert.Equal("hello multiple interface", result.Value2);

      var result2 = (OldTypes.IGeneric2<string, int>)container.Resolve(typeof(OldTypes.IGeneric2<string, int>));
      Assert.Equal("hello multiple interface", result2.Value1);
      Assert.Equal(70, result2.Value2);
    }

    [Fact]
    public void ShouldResolveGenericTypeWith2ReverseParametersByInterface()
    {
      var container = CreateContainer();
      container.Register((80).AsObjectTarget());
      container.Register("hello reversed interface".AsObjectTarget());
      //the thing here being that the type parameters for IGeneric2 are swapped in Generic2Reversed,
      //so we're testing that the engine can identify that and map the parameters from IGeneric2
      //back to the type parameters that should be passed to Generic2Reversed
      container.Register(GenericConstructorTarget.Auto(typeof(Generic2Reversed<,>)), typeof(OldTypes.IGeneric2<,>));
      var result = (OldTypes.IGeneric2<int, string>)container.Resolve(typeof(OldTypes.IGeneric2<int, string>));
      Assert.IsType<Generic2Reversed<string, int>>(result);
      Assert.Equal(80, result.Value1);
      Assert.Equal("hello reversed interface", result.Value2);
    }

    [Fact]
    public void ShouldResolveGenericTypeByABase()
    {
      var container = CreateContainer();
      container.Register((90).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric<>)), typeof(OldTypes.Generic<>));
      var result = (OldTypes.Generic<int>)container.Resolve(typeof(OldTypes.Generic<int>));
      Assert.Equal(90, result.Value);
    }

    [Fact]
    public void ShouldResolveGenericTypeByABaseOfABase()
    {
      //testing that the type parameter mapping will walk the inheritance treee correctly
      var container = CreateContainer();
      container.Register((100).AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DoubleDeriveGeneric<>)), typeof(OldTypes.Generic<>));
      var result = (OldTypes.Generic<int>)container.Resolve(typeof(OldTypes.Generic<int>));
      Assert.Equal(100, result.Value);
    }

    [Fact]
    public void ShouldResolveGenericWithTwoParametersByABase()
    {
      var container = CreateContainer();
      container.Register((110).AsObjectTarget());
      container.Register("Hello double parameter base".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2<,>)), typeof(OldTypes.Generic2<,>));
      var result = (OldTypes.Generic2<int, string>)container.Resolve(typeof(OldTypes.Generic2<int, string>));
      Assert.Equal(110, result.Value1);
      Assert.Equal("Hello double parameter base", result.Value2);
    }

    [Fact]
    public void ShouldResolveGenericWithTwoParametersByABaseOfABase()
    {
      var container = CreateContainer();
      container.Register((120).AsObjectTarget());
      container.Register("Hello double parameter base of a base".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2<,>)), typeof(OldTypes.Generic2<,>));
      var result = (OldTypes.Generic2<int, string>)container.Resolve(typeof(OldTypes.Generic2<int, string>));
      Assert.Equal(120, result.Value1);
      Assert.Equal("Hello double parameter base of a base", result.Value2);
    }

    [Fact]
    public void ShouldResolveGenericWithTwoParametersReversedByItsBase()
    {
      var container = CreateContainer();
      container.Register((130).AsObjectTarget());
      container.Register("Hello double parameter reversed base".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2Reversed<,>)), typeof(OldTypes.Generic2<,>));
      var result = (OldTypes.Generic2<int, string>)container.Resolve(typeof(OldTypes.Generic2<int, string>));
      Assert.IsType<DerivedGeneric2Reversed<string, int>>(result);
      Assert.Equal(130, result.Value1);
      Assert.Equal("Hello double parameter reversed base", result.Value2);
    }

    [Fact]
    public void ShouldResolveGenericWithTwoParametersReversedByABaseOfItsBase()
    {
      var container = CreateContainer();
      container.Register((140).AsObjectTarget());
      container.Register("Hello double parameter reversed base of a base".AsObjectTarget());
      container.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2Reversed<,>)), typeof(OldTypes.Generic2<,>));
      var result = (OldTypes.Generic2<int, string>)container.Resolve(typeof(OldTypes.Generic2<int, string>));
      Assert.IsType<DoubleDerivedGeneric2Reversed<string, int>>(result);
      Assert.Equal(140, result.Value1);
      Assert.Equal("Hello double parameter reversed base of a base", result.Value2);
    }


  }
}
