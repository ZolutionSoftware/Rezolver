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

	public abstract class AbstractGeneric<T>
	{
		protected abstract void Foo();
	}

	public class Generic<T> : AbstractGeneric<T>
	{
		protected override void Foo()
		{
			throw new NotImplementedException();
		}
	}
}
