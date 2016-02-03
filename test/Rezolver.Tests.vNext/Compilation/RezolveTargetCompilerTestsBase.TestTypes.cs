using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.vNext.Compilation
{
    public abstract partial class RezolveTargetCompilerTestsBase
    {
		public interface ITransient
		{
		}

		public class Transient : ITransient
		{
			public static int Counter = 0;

			public Transient()
			{
				++Counter;
			}
		}

		public interface IRequiresInt
		{
			int Int { get; }
		}

		public class RequiresInt : IRequiresInt
		{
			public RequiresInt(int @int)
			{
				Int = @int;
			}

			public int Int { get; private set; }
		}

		public interface IRequiresNullableInt
		{
			int? NullableInt { get; }
		}

		public class RequiresNullableInt : IRequiresNullableInt
		{
			public int? NullableInt { get; private set; }

			public RequiresNullableInt(int? nullableInt)
			{
				NullableInt = nullableInt;
			}
		}

		public interface ISingleton { }

		public class Singleton : ISingleton
		{
			public static int Counter = 0;

			public Singleton()
			{
				++Counter;
			}
		}

		public interface IComposite
		{
			ISingleton Singleton { get; }
			ITransient Transient { get; }
		}

		public class Composite : IComposite
		{
			private readonly ISingleton _singleton;
			private readonly ITransient _transient;

			public Composite(ISingleton singleton, ITransient transient)
			{
				_singleton = singleton;
				_transient = transient;
			}

			public ISingleton Singleton
			{
				get { return _singleton; }
			}

			public ITransient Transient
			{
				get { return _transient; }
			}
		}

		public class HasProperty
		{
			public IRequiresInt RequiresInt { get; set; }
		}

		/// <summary>
		/// interface for the type that will be used to test the registration and compilation of all the different target types.
		/// </summary>
		public interface ISuperComplex
		{
			int Int { get; }
			int? NullableInt { get; }
			string String { get; }
			ITransient Transient { get; }
			ISingleton Singleton { get; }
			IComposite Composite { get; }
		}

		public class SuperComplex : ISuperComplex
		{
			public SuperComplex(int @int,
				int? nullableInt,
				string @string,
				ITransient transient,
				ISingleton singleton,
				IComposite composite)
			{
				Int = @int;
				NullableInt = nullableInt;
				String = @string;
				Transient = transient;
				Singleton = singleton;
				Composite = composite;
			}

			public int Int { get; private set; }
			public int? NullableInt { get; private set; }
			public string String { get; private set; }
			public ITransient Transient { get; private set; }
			public ISingleton Singleton { get; private set; }
			public IComposite Composite { get; private set; }
		}

		public class ScopedSingletonTestClass
		{

		}
	}
}
