using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
			var rezolver = new Container(compiler: new TargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));
			var instance = (GenericNoCtor<int>)rezolver.Resolve(typeof(GenericNoCtor<int>));
			Assert.NotNull(instance);
		}

		[Fact]
		public void ShouldCreateAGenericClass()
		{
			//typical pattern already 
			var rezolver = CreateADefaultRezolver();
			rezolver.RegisterObject(1);
			ITarget t = GenericConstructorTarget.Auto(typeof(Generic<>));
			Assert.NotNull(t);
			Assert.Equal(typeof(Generic<>), t.DeclaredType);
			var instance = GetValueFromTarget<Generic<int>>(t, rezolver);
			Assert.NotNull(instance);
			Assert.Equal(1, instance.Value);
		}

		[Fact]
		public void ShouldRezolveAGenericClass()
		{
			//in this one, using DefaultRezolver, we're going to test a few parameter types
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register((3).AsObjectTarget(typeof(int?)));
			rezolver.Register("hello world".AsObjectTarget());
			var instance1 = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			var instance2 = (Generic<string>)rezolver.Resolve(typeof(Generic<string>));
			var instance3 = (Generic<int?>)rezolver.Resolve(typeof(Generic<int?>));

			Assert.Equal(2, instance1.Value);
			Assert.Equal("hello world", instance2.Value);
			Assert.Equal(3, instance3.Value);
		}

		//now test that the target should work when used as the target of a dependency look up
		//just going to use the DefaultRezolver for this as it's far easier to setup the test.

		[Fact]
		public void ShouldRezolveAClosedGenericDependency()
		{
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasGenericDependency>());

			var result = (HasGenericDependency)rezolver.Resolve(typeof(HasGenericDependency));
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
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());

			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));

			var result = (Generic<GenericNoCtor<int>>)rezolver.Resolve(typeof(Generic<GenericNoCtor<int>>));
			Assert.NotNull(result);
			Assert.NotNull(result.Value);
			Assert.Equal(0, result.Value.Value);
		}


		//this one is the open generic nested dependency check

		[Fact]
		public void ShouldResolveNestedeOpenGenericDependency()
		{
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());

			rezolver.Register((10).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericDependency<>)));

			var result = (HasOpenGenericDependency<int>)rezolver.Resolve(typeof(HasOpenGenericDependency<int>));
			Assert.NotNull(result);
			Assert.NotNull(result.Dependency);
			Assert.Equal(10, result.Dependency.Value);
		}

		//now moving on to rezolving interface instead of the type directly

		[Fact]
		public void ShouldResolveGenericViaInterface()
		{
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());
			rezolver.Register((20).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));

			var result = (IGeneric<int>)rezolver.Resolve(typeof(IGeneric<int>));
			Assert.IsType<Generic<int>>(result);
			Assert.Equal(20, result.Value);
		}

		[Fact]
		public void ShouldRezolveGenericViaGenericInterface()
		{
			//first version of this test - where the nested generic interface is different
			//to the outer generic interface.  At the time of writing, making it the same causes
			//a circular dependency - see Bug #7

			IContainer rezolver = CreateADefaultRezolver();
			//we need three dependencies registered - the inner T, an IGenericA<> and 
			//an IGeneric<IGenericA<T>>.
			rezolver.Register((25).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			//note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
			//supply an open generic as a type parameter to a generic is not valid.
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericGeneric<>)), typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));

			var result = (IGeneric<IGeneric<int>>)rezolver.Resolve(typeof(IGeneric<IGeneric<int>>));

			Assert.Equal(25, result.Value.Value);
			Assert.IsType<GenericGeneric<int>>(result);
			Assert.IsType<Generic<int>>(result.Value);
		}

		[Fact]
		public void ShouldResolveClosedGenericViaInterfaceDependency()
		{
			IContainer rezolver = new Container(compiler: new TargetDelegateCompiler());
			rezolver.Register((30).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(ConstructorTarget.Auto<HasGenericInterfaceDependency>());
			var result = (HasGenericInterfaceDependency)rezolver.Resolve(typeof(HasGenericInterfaceDependency));
			Assert.NotNull(result.Dependency);
			Assert.Equal(30, result.Dependency.Value);
		}

		[Fact]
		public void ShouldResolveOpenGenericViaInterfaceDependency()
		{
			IContainer rezolver = CreateADefaultRezolver();
			rezolver.Register((40).AsObjectTarget());
			rezolver.Register((50d).AsObjectTarget(typeof(double?))); //will that work?
			rezolver.Register("hello interface generics!".AsObjectTarget());
			//now register the IGeneric<T>
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericInterfaceDependency<>)));
			var resultWithInt = (HasOpenGenericInterfaceDependency<int>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<int>));
			var resultWithNullableDouble = (HasOpenGenericInterfaceDependency<double?>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<double?>));
			var resultWithString = (HasOpenGenericInterfaceDependency<string>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<string>));
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
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((60).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.Equal(60, result.Value1);
			Assert.Equal("hello multiple", result.Value2);

			var result2 = (Generic2<string, int>)rezolver.Resolve(typeof(Generic2<string, int>));
			Assert.Equal("hello multiple", result2.Value1);
			Assert.Equal(60, result2.Value2);
		}

		//TODO: resolve by base (DerivedGeneric<T>) and then probably using reversed parameters also.
		[Fact]
		public void ShouldResolveGenericTypeWith2ParametersByInterface()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((70).AsObjectTarget());
			rezolver.Register("hello multiple interface".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)), typeof(IGeneric2<,>));
			var result = (IGeneric2<int, string>)rezolver.Resolve(typeof(IGeneric2<int, string>));
			Assert.Equal(70, result.Value1);
			Assert.Equal("hello multiple interface", result.Value2);

			var result2 = (IGeneric2<string, int>)rezolver.Resolve(typeof(IGeneric2<string, int>));
			Assert.Equal("hello multiple interface", result2.Value1);
			Assert.Equal(70, result2.Value2);
		}

		[Fact]
		public void ShouldResolveGenericTypeWith2ReverseParametersByInterface()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((80).AsObjectTarget());
			rezolver.Register("hello reversed interface".AsObjectTarget());
			//the thing here being that the type parameters for IGeneric2 are swapped in Generic2Reversed,
			//so we're testing that the engine can identify that and map the parameters from IGeneric2
			//back to the type parameters that should be passed to Generic2Reversed
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2Reversed<,>)), typeof(IGeneric2<,>));
			var result = (IGeneric2<int, string>)rezolver.Resolve(typeof(IGeneric2<int, string>));
			Assert.IsType<Generic2Reversed<string, int>>(result);
			Assert.Equal(80, result.Value1);
			Assert.Equal("hello reversed interface", result.Value2);
		}

		[Fact]
		public void ShouldResolveGenericTypeByABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((90).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric<>)), typeof(Generic<>));
			var result = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			Assert.Equal(90, result.Value);
		}

		[Fact]
		public void ShouldResolveGenericTypeByABaseOfABase()
		{
			//testing that the type parameter mapping will walk the inheritance treee correctly
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((100).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDeriveGeneric<>)), typeof(Generic<>));
			var result = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			Assert.Equal(100, result.Value);
		}

		[Fact]
		public void ShouldResolveGenericWithTwoParametersByABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((110).AsObjectTarget());
			rezolver.Register("Hello double parameter base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.Equal(110, result.Value1);
			Assert.Equal("Hello double parameter base", result.Value2);
		}

		[Fact]
		public void ShouldResolveGenericWithTwoParametersByABaseOfABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((120).AsObjectTarget());
			rezolver.Register("Hello double parameter base of a base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.Equal(120, result.Value1);
			Assert.Equal("Hello double parameter base of a base", result.Value2);
		}

		[Fact]
		public void ShouldResolveGenericWithTwoParametersReversedByItsBase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((130).AsObjectTarget());
			rezolver.Register("Hello double parameter reversed base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2Reversed<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.IsType<DerivedGeneric2Reversed<string, int>>(result);
			Assert.Equal(130, result.Value1);
			Assert.Equal("Hello double parameter reversed base", result.Value2);
		}

		[Fact]
		public void ShouldResolveGenericWithTwoParametersReversedByABaseOfItsBase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((140).AsObjectTarget());
			rezolver.Register("Hello double parameter reversed base of a base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2Reversed<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.IsType<DoubleDerivedGeneric2Reversed<string, int>>(result);
			Assert.Equal(140, result.Value1);
			Assert.Equal("Hello double parameter reversed base of a base", result.Value2);
		}


	}
}
