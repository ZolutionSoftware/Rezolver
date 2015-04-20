using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Rezolver.Examples
{
	[TestClass]
	public class MultipleResolveExamples
	{
		public interface IFoo
		{
			string Message { get; }
		}

		public class Foo1 : IFoo
		{
			public string Message
			{
				get { return "Hello World 1"; }
			}
		}

		public class Foo2 : IFoo
		{

			public string Message
			{
				get { return "Hello World 2"; }
			}
		}

		public class Foo3 : IFoo
		{

			public string Message
			{
				get { return "Hello World 3"; }
			}
		}

		public class Bar
		{
			public IEnumerable<IFoo> Foos { get; private set; }
			public Bar(IEnumerable<IFoo> foos)
			{
				Foos = foos;
			}

			public IEnumerable<string> Messages
			{
				get
				{
					return Foos.Select(f => f.Message);
				}
			}
		}

		[TestMethod]
		public void ResolveMultipleFooDependency()
		{
			var rezolver = new DefaultRezolver();
			rezolver.RegisterType<Bar>();
			//for this, we have to create targets directly
			//so, below, we're using the ConstructorTarget - 
			//which is what the RegisterType<T> extension method,
			//that we've already been using, does
			rezolver.RegisterMultiple(new[] 
			{
				ConstructorTarget.Auto<Foo1>(),
				ConstructorTarget.Auto<Foo2>(),
				ConstructorTarget.Auto<Foo3>()
			}, typeof(IFoo));

			var result = rezolver.Resolve<Bar>();
			Assert.IsTrue(result.Messages.SequenceEqual(new[]{
				"Hello World 1",
				"Hello World 2",
				"Hello World 3"
			}));
		}
	}
}
