using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class OpenGenericTests
	{
		public interface IGeneric<T>
		{
			T Value { get; }
		}

		public class Generic<T> : IGeneric<T>
		{
			private T _value;

			public Generic(T value)
			{
				_value = value;
			}

			public T Value
			{
				get { return _value; }
			}
		}

		[TestMethod]
		public void ShouldRegisterAsOpenGeneric()
		{
			var r = new RezolverScope();
			//r.Register();
		}
	}
}
