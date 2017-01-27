using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Targets
{
	public class NoCtor { }

	public class OneCtor
	{
		public OneCtor(int param1)
		{

		}
	}

	public class TwoCtors
	{
		public TwoCtors(string s) { }
		public TwoCtors(string s, int i) { }
	}

	public class TwoCtorsOneNoOptional
	{
		//signatures here have to be slightly different obviously

		public TwoCtorsOneNoOptional(string s, int i, object o) { }
		public TwoCtorsOneNoOptional(string s, int i = 0, double d = 0) { }
	}

	public class Decorated
	{

	}

	public class Decorator : Decorated
	{
		public Decorated Decorated { get; }
		public Decorator(Decorated decorated)
		{
			Decorated = decorated;
		}
	}

	#region Generic test types

	/// <summary>
	/// some types to use as generic arguments for our generic tests
	/// </summary>
	public class TypeArgs
	{
		public interface IT1 { }
		public interface IT2 { }
		public interface IT3 { }

		public class T1 : IT1 { }
		public class T2 : IT2 { }
		public class T3 : IT3 { }

		public struct VT1 : IT1 { }
		public struct VT2 : IT2 { }
		public struct VT3 : IT3 { }
	}

	public class GenericDecorator<TDecorated> : Decorated
		where TDecorated : Decorated
	{
		public TDecorated Decorated { get; }
		public GenericDecorator(TDecorated decorated)
		{
			Decorated = decorated;
		}
	}

	public interface IGeneric<T>
	{
		void Foo();
	}

	public abstract class GenericBase<T> : IGeneric<T>
	{
		public abstract void Foo();
	}

	public class Generic<T> : GenericBase<T>
	{
		public override void Foo()
		{
			throw new NotImplementedException();
		}
	}

	//two param generic - note that you can't get an instance
	//by requesting IGeneric<[sometype]> because there's no way to know
	//the type argument to use for the other parameters.
	public interface IGeneric2<Ta, Tb> : IGeneric<Ta>
	{
		void Foo2();
	}

	public class Generic2<Ta, Tb> : GenericBase<Ta>, IGeneric2<Ta, Tb>
	{
		public override void Foo()
		{

		}

		public void Foo2() { }
	}

	public class ReversingGeneric2<Ta, Tb> : Generic2<Tb, Ta>
	{

	}

	//the generic type you request from the generic constructor target must have at
	//least as many generic arguments as the type to which its bound. 
	//I.E you can't register Foo<T,U> : IBar<T> to IBar<> because when you request 
	//IBar<Baz>, the system cannot bind the 'U' type parameter for Foo<,>
	//However, if you register Bar<T> : IFoo<T, int> for IFoo<,> then you can request 
	//IFoo<double, int>, because the second type parameter is statically mapped.

	public interface INarrowingGeneric<T> : IGeneric2<T, TypeArgs.T2>
	{

	}

	public class NarrowingGeneric<T> : Generic2<T, TypeArgs.T2>, INarrowingGeneric<T>
	{
		//if you register this type and request an IGeneric<*, T2> it should work.
		//equally, if you request an IGeneric<*> it should also work.
	}

	public class NestedGenericA<T> : Generic<IEnumerable<T>>
	{
		//finding this type's mapping to any of the bases or interfaces involves 
		//translating through the layers of inheritance/implementation and 
		//getting to the IEnumerable type parameter, then lifting the T out
	}

	public interface INestedGenericB<T> : IGeneric<IEnumerable<T>>
	{

	}

	public class NestedGenericB<T> : INestedGenericB<T>
	{
		public void Foo()
		{
			throw new NotImplementedException();
		}
	}

	public class TwiceNestedGenericA<T> : IGeneric<IGeneric<IEnumerable<T>>>
	{
		//even more complicated now - double nesting of generic parameters
		public void Foo()
		{
			throw new NotImplementedException();
		}
	}

	public interface ITwiceNestedGenericB<T> : IGeneric<IGeneric<IEnumerable<T>>>
	{

	}

	public class TwiceNestedGenericB<T> : ITwiceNestedGenericB<T>
	{
		public void Foo()
		{
			throw new NotImplementedException();
		}
	}

	public struct GenericValueType<T> { }

	#endregion
}
