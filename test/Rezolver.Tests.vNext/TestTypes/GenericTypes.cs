using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.vNext.TestTypes
{
	//TODO: Split each declaration into its own file.

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

	public class DerivedGeneric2<T7, U4> : Generic2<T7, U4>
	{
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
}
