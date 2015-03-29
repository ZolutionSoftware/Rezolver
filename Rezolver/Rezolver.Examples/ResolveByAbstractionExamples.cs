using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Examples
{
	[TestClass]
	public class ResolveByAbstractionExamples
	{
		public abstract class Abstraction1Base
		{
			public abstract string Message { get; }
		}

		//we'll register this type against its abstract base type only
		public class Abstraction1 : Abstraction1Base
		{

			public override string Message
			{
				get { return "Abstraction1"; }
			}
		}

		public interface IAbstraction2
		{
			string Message { get; }
		}

		//this will be registered by its interface type only
		public class Abstraction2 : IAbstraction2
		{

			public string Message
			{
				get { return "Abstraction2"; }
			}
		}

		public interface IAbstraction3
		{
			string Message1 { get; }
			string Message2 { get; }
		}

		//we'll then resolve this by the third abstraction - another interface
		public class RequiresAbstractions : IAbstraction3
		{
			private readonly Abstraction1Base _abstraction1Base;
			private readonly IAbstraction2 _abstraction2;

			public string Message1
			{
				get { return _abstraction1Base.Message; }
			}

			public string Message2
			{
				get { return _abstraction2.Message; }
			}

			//dependencies here are expressed in terms of the abstractions, not the concrete types
			public RequiresAbstractions(Abstraction1Base abstraction1Base, IAbstraction2 abstraction2)
			{
				Assert.IsNotNull(abstraction1Base);
				Assert.IsNotNull(abstraction2);

				_abstraction1Base = abstraction1Base;
				_abstraction2 = abstraction2;
			}
		}

		[TestMethod]
		public void ByAbstraction()
		{
			var resolver = new DefaultRezolver();

			//the RegisterType extension has a generic overload that accepts both the concrete
			//type you want to construct (first arg) and the abstraction type you want to register
			//it for (i.e. the type you want to resolve as an instance of the concrete type).
			//the overload is type safe - only compiling if the first arg is derived from, or implements
			//the second.
			resolver.RegisterType<Abstraction1, Abstraction1Base>();
			resolver.RegisterType<Abstraction2, IAbstraction2>();
			resolver.RegisterType<RequiresAbstractions, IAbstraction3>();
			
			var result = resolver.Resolve<IAbstraction3>();
			Assert.AreEqual("Abstraction1", result.Message1);
			Assert.AreEqual("Abstraction2", result.Message2);
		}
	}
}
