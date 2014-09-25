using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class GenericConstructorTargetTests : TestsBase
	{
		//please note - each and every generic is using uniquely named generic types to ensure that no false positives
		//are reported in the tests.

		public interface IGeneric<T>
		{
			T Value { get; }
		}

		/// <summary>
		/// alternative IGeneric-like interface used to simplify the nested open generic scenario
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public interface IGenericA<TA>
		{
			TA Value { get; }
		}

		public interface IGeneric2<T2, U2> : IGeneric<U2>
		{
			T2 Value1 { get; }
			U2 Value2 { get; }
		}

		public class GenericNoCtor<T3> : IGeneric<T3>
		{
			public T3 Value { get; set; }
		}

		public class Generic<T4> : IGeneric<T4>
		{
			private T4 _value;

			public Generic(T4 value)
			{
				_value = value;
			}

			public T4 Value
			{
				get { return _value; }
			}
		}

		public class GenericA<TA2> : IGenericA<TA2>
		{
			private TA2 _value;

			public GenericA(TA2 value)
			{
				_value = value;
			}

			public TA2 Value
			{
				get { return _value; }
			}
		}

		/// <summary>
		/// this is pretty hideous - but might be something that needs to be supported
		/// 
		/// pushes the discovery of type parameters by forcing unwrap another nested generic type parameter.
		/// </summary>
		/// <typeparam name="T5"></typeparam>
		public class GenericGeneric<T5> : IGeneric<IGeneric<T5>>
		{

			public IGeneric<T5> Value
			{
				get;
				private set;
			}

			public GenericGeneric(IGeneric<T5> value)
			{
				Value = value;
			}
		}

		public class Generic2<T6, U3> : IGeneric2<T6, U3>
		{
			public Generic2(T6 value1, U3 value2)
			{
				Value1 = value1;
				Value2 = value2;
			}

			public T6 Value1
			{
				get;
				private set;
			}

			public U3 Value2
			{
				get;
				private set;
			}

			//explicit implementation of IGeneric<U>
			U3 IGeneric<U3>.Value
			{
				get { return Value2; }
			}
		}

		public class DerivedGeneric2<T7, U4> : Generic2<T7, U4> {
			public DerivedGeneric2(T7 value1, U4 value2) : base(value1, value2) { }
		}

		public class DoubleDerivedGeneric2<T8, U5> : DerivedGeneric2<T8, U5>
		{
			public DoubleDerivedGeneric2(T8 value1, U5 value2) : base(value1, value2) { }
		}

		public class Generic2Reversed<T9, U6> : IGeneric2<U6, T9>
		{
			public Generic2Reversed(T9 value2, U6 value1)
			{
				Value1 = value1;
				Value2 = value2;
			}

			public U6 Value1
			{
				get;
				private set;
			}

			public T9 Value2
			{
				get;
				private set;
			}

			//explicit implementation of IGeneric<T>
			T9 IGeneric<T9>.Value
			{
				get { return Value2; }
			}
		}

		public class DerivedGeneric2Reversed<T10, U7> : Generic2<U7, T10>
		{
			public DerivedGeneric2Reversed(T10 value1, U7 value2) : base(value2, value1) { }
		}

		public class DoubleDerivedGeneric2Reversed<T11, U8> : DerivedGeneric2Reversed<T11, U8>
		{
			public DoubleDerivedGeneric2Reversed(T11 value1, U8 value2) : base(value1, value2) { }
		}

		public class DerivedGeneric<T12> : Generic<T12>
		{
			public DerivedGeneric(T12 value) : base(value) { }
		}

		public class DoubleDeriveGeneric<T13> : DerivedGeneric<T13>
		{
			public DoubleDeriveGeneric(T13 value) : base(value) { }
		}

		public class HasGenericDependency
		{
			public Generic<int> Dependency { get; private set; }
			public HasGenericDependency(Generic<int> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasOpenGenericDependency<T14>
		{
			public Generic<T14> Dependency { get; private set; }
			public HasOpenGenericDependency(Generic<T14> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasGenericInterfaceDependency
		{
			public IGeneric<int> Dependency { get; private set; }
			public HasGenericInterfaceDependency(IGeneric<int> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasOpenGenericInterfaceDependency<T15>
		{
			public IGeneric<T15> Dependency { get; private set; }
			public HasOpenGenericInterfaceDependency(IGeneric<T15> dependency)
			{
				Dependency = dependency;
			}
		}

		[TestMethod]
		public void ShouldCreateGenericNoCtorClass()
		{
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(GenericNoCtor<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(GenericNoCtor<>), t.DeclaredType);
			//try and build an instance 
			var instance = GetValueFromTarget<GenericNoCtor<int>>(t);
			Assert.IsNotNull(instance);
			Assert.AreEqual(default(int), instance.Value);
		}

		[TestMethod]
		public void ShouldResolveAGenericNoCtorClass()
		{
			//similar test to above, but we're testing that it works when you put the target inside the
			//default resolver.
			var rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));
			var instance = (GenericNoCtor<int>)rezolver.Resolve(typeof(GenericNoCtor<int>));
			Assert.IsNotNull(instance);
		}

		[TestMethod]
		public void ShouldCreateAGenericClass()
		{
			//use a rezolver mock for cross-referencing the int parameter
			var builderMock = new Mock<IRezolverBuilder>();
			var rezolverMock = new Mock<IRezolver>();
			builderMock.Setup(r => r.Fetch(typeof(int), It.IsAny<string>())).Returns((1).AsObjectTarget());
			rezolverMock.Setup(r => r.Builder).Returns(builderMock.Object);
			rezolverMock.Setup(r => r.Compiler).Returns(new RezolveTargetDelegateCompiler());
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(Generic<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(Generic<>), t.DeclaredType);
			var instance = GetValueFromTarget<Generic<int>>(t, rezolverMock.Object);
			Assert.IsNotNull(instance);
			Assert.AreEqual(1, instance.Value);
		}

		[TestMethod]
		public void ShouldRezolveAGenericClass()
		{
			//in this one, using DefaultRezolver, we're going to test a few parameter types
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register((3).AsObjectTarget(typeof(int?)));
			rezolver.Register("hello world".AsObjectTarget());
			var instance1 = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			var instance2 = (Generic<string>)rezolver.Resolve(typeof(Generic<string>));
			var instance3 = (Generic<int?>)rezolver.Resolve(typeof(Generic<int?>));

			Assert.AreEqual(2, instance1.Value);
			Assert.AreEqual("hello world", instance2.Value);
			Assert.AreEqual(3, instance3.Value);
		}

		//now test that the target should work when used as the target of a dependency look up
		//just going to use the DefaultRezolver for this as it's far easier to setup the test.

		[TestMethod]
		public void ShouldRezolveAClosedGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasGenericDependency>());

			var result = (HasGenericDependency)rezolver.Resolve(typeof(HasGenericDependency));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(2, result.Dependency.Value);
		}

		[TestMethod]
		public void ShouldRezolveNestedGenericDependency()
		{
			//this one is more complicated.  Passing a closed generic as a type argument to another
			//generic.
			//note that this isn't the most complicated it can get, however: that would be using
			//the type argument as a type argument to another open generic dependency.  That one is on it's way.
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));

			var result = (Generic<GenericNoCtor<int>>)rezolver.Resolve(typeof(Generic<GenericNoCtor<int>>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Value);
			Assert.AreEqual(0, result.Value.Value);
		}


		//this one is the open generic nested dependency check

		[TestMethod]
		public void ShouldResolveNestedeOpenGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register((10).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericDependency<>)));

			var result = (HasOpenGenericDependency<int>)rezolver.Resolve(typeof(HasOpenGenericDependency<int>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(10, result.Dependency.Value);
		}

		//now moving on to rezolving interface instead of the type directly

		[TestMethod]
		public void ShouldResolveGenericViaInterface()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((20).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));

			var result = (IGeneric<int>)rezolver.Resolve(typeof(IGeneric<int>));
			Assert.IsInstanceOfType(result, typeof(Generic<int>));
			Assert.AreEqual(20, result.Value);
		}

		[TestMethod]
		public void ShouldRezolveGenericViaGenericInterface()
		{
			//first version of this test - where the nested generic interface is different
			//to the outer generic interface.  At the time of writing, making it the same causes
			//a circular dependency - see Bug #7

			IRezolver rezolver = CreateADefaultRezolver();
			//we need three dependencies registered - the inner T, an IGenericA<> and 
			//an IGeneric<IGenericA<T>>.
			rezolver.Register((25).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			//note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
			//supply an open generic as a type parameter to a generic is not valid.
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericGeneric<>)), typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));

			var result = (IGeneric<IGeneric<int>>)rezolver.Resolve(typeof(IGeneric<IGeneric<int>>));

			Assert.AreEqual(25, result.Value.Value);
			Assert.IsInstanceOfType(result, typeof(GenericGeneric<int>));
			Assert.IsInstanceOfType(result.Value, typeof(Generic<int>));
		}

		[TestMethod]
		public void ShouldResolveClosedGenericViaInterfaceDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((30).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(ConstructorTarget.Auto<HasGenericInterfaceDependency>());
			var result = (HasGenericInterfaceDependency)rezolver.Resolve(typeof(HasGenericInterfaceDependency));
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(30, result.Dependency.Value);
		}

		[TestMethod]
		public void ShouldResolveOpenGenericViaInterfaceDependency()
		{
			IRezolver rezolver = CreateADefaultRezolver();
			rezolver.Register((40).AsObjectTarget());
			rezolver.Register((50d).AsObjectTarget(typeof(double?))); //will that work?
			rezolver.Register("hello interface generics!".AsObjectTarget());
			//now register the IGeneric<T>
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericInterfaceDependency<>)));
			var resultWithInt = (HasOpenGenericInterfaceDependency<int>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<int>));
			var resultWithNullableDouble = (HasOpenGenericInterfaceDependency<double?>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<double?>));
			var resultWithString = (HasOpenGenericInterfaceDependency<string>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<string>));
			Assert.IsNotNull(resultWithInt);
			Assert.IsNotNull(resultWithNullableDouble);
			Assert.IsNotNull(resultWithString);
			Assert.IsNotNull(resultWithInt.Dependency);
			Assert.IsNotNull(resultWithNullableDouble.Dependency);
			Assert.IsNotNull(resultWithString.Dependency);
			Assert.AreEqual(40, resultWithInt.Dependency.Value);
			Assert.AreEqual(50, resultWithNullableDouble.Dependency.Value);
			Assert.AreEqual("hello interface generics!", resultWithString.Dependency.Value);
		}

		//now on to the multiple type parameters
		//first by direct type
		[TestMethod]
		public void ShouldResolveGenericTypeWith2Parameters()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((60).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.AreEqual(60, result.Value1);
			Assert.AreEqual("hello multiple", result.Value2);

			var result2 = (Generic2<string, int>)rezolver.Resolve(typeof(Generic2<string, int>));
			Assert.AreEqual("hello multiple", result2.Value1);
			Assert.AreEqual(60, result2.Value2);
		}

		//TODO: resolve by base (DerivedGeneric<T>) and then probably using reversed parameters also.
		[TestMethod]
		public void ShouldResolveGenericTypeWith2ParametersByInterface()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((70).AsObjectTarget());
			rezolver.Register("hello multiple interface".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)), typeof(IGeneric2<,>));
			var result = (IGeneric2<int, string>)rezolver.Resolve(typeof(IGeneric2<int, string>));
			Assert.AreEqual(70, result.Value1);
			Assert.AreEqual("hello multiple interface", result.Value2);

			var result2 = (IGeneric2<string, int>)rezolver.Resolve(typeof(IGeneric2<string, int>));
			Assert.AreEqual("hello multiple interface", result2.Value1);
			Assert.AreEqual(70, result2.Value2);
		}

		[TestMethod]
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
			Assert.IsInstanceOfType(result, typeof(Generic2Reversed<string, int>));
			Assert.AreEqual(80, result.Value1);
			Assert.AreEqual("hello reversed interface", result.Value2);
		}

		[TestMethod]
		public void ShouldResolveGenericTypeByABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((90).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric<>)), typeof(Generic<>));
			var result = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			Assert.AreEqual(90, result.Value);
		}

		[TestMethod]
		public void ShouldResolveGenericTypeByABaseOfABase()
		{
			//testing that the type parameter mapping will walk the inheritance treee correctly
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((100).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDeriveGeneric<>)), typeof(Generic<>));
			var result = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			Assert.AreEqual(100, result.Value);
		}

		[TestMethod]
		public void ShouldResolveGenericWithTwoParametersByABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((110).AsObjectTarget());
			rezolver.Register("Hello double parameter base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.AreEqual(110, result.Value1);
			Assert.AreEqual("Hello double parameter base", result.Value2);
		}

		[TestMethod]
		public void ShouldResolveGenericWithTwoParametersByABaseOfABase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((120).AsObjectTarget());
			rezolver.Register("Hello double parameter base of a base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.AreEqual(120, result.Value1);
			Assert.AreEqual("Hello double parameter base of a base", result.Value2);
		}

		[TestMethod]
		public void ShouldResolveGenericWithTwoParametersReversedByItsBase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((130).AsObjectTarget());
			rezolver.Register("Hello double parameter reversed base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DerivedGeneric2Reversed<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.IsInstanceOfType(result, typeof(DerivedGeneric2Reversed<string, int>));
			Assert.AreEqual(130, result.Value1);
			Assert.AreEqual("Hello double parameter reversed base", result.Value2);
		}

		[TestMethod]
		public void ShouldResolveGenericWithTwoParametersReversedByABaseOfItsBase()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((140).AsObjectTarget());
			rezolver.Register("Hello double parameter reversed base of a base".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(DoubleDerivedGeneric2Reversed<,>)), typeof(Generic2<,>));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.IsInstanceOfType(result, typeof(DoubleDerivedGeneric2Reversed<string, int>));
			Assert.AreEqual(140, result.Value1);
			Assert.AreEqual("Hello double parameter reversed base of a base", result.Value2);
		}

		//should we be thinking about covariance?
	}
}
