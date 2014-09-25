using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRezolveTests
	{
		public class Foo
		{
			public string Name { get; private set; }
			public Foo(string name = null)
			{
				Name = name;
			}
		}

		public class Bar
		{
			public Foo Foo { get; private set; }
			public Bar(Foo foo)
			{
				Foo = foo;
			}
		}

		[TestMethod]
		public void ShouldResolveNamedObject1()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path: "name1");
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1");
			Assert.AreEqual("name1", result.Name);
		}

		[TestMethod]
		public void ShouldResolveNamedObject2()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path: "name1.name2");
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1.name2");
			Assert.AreEqual("name1.name2", result.Name);
		}

		[TestMethod]
		public void ShouldResolveNamedObject3Fallback()
		{
			//feeding a name and getting the fallback
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)));
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1");
			Assert.AreEqual("name1", result.Name);
		}

		[TestMethod]
		public void ShouldResolveNamedObject4Fallback()
		{
			//feeding a name and getting the fallback
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)));
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1.name2");
			Assert.AreEqual("name1.name2", result.Name);
		}

		[TestMethod]
		public void ShouldResolveNamedObject5Fallback()
		{
			//feeding a name and getting the fallback
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path:"name1");
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1.name2");
			Assert.AreEqual("name1.name2", result.Name);
		}

		[TestMethod]
		public void ShouldResolveNamedObject6Fallback()
		{
			//feeding a name and getting the fallback
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path:"name1.name2");
			var result = (Foo)rezolver.Resolve(typeof(Foo), "name1.name2.name3.name4");
			Assert.AreEqual("name1.name2.name3.name4", result.Name);
		}

		[TestMethod]
		public void NamedObjectShouldResolveUnnamedDependency1()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Builder.Register(ConstructorTarget.For<Foo>((c) => new Foo(null)));
			rezolver.Builder.Register(ConstructorTarget.Auto<Bar>(), path: "name1");

			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1");
			Assert.IsNull(result.Foo.Name);
		}

		[TestMethod]
		public void NamedObjectShouldResolveUnnamedDependency2()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Builder.Register(ConstructorTarget.For<Foo>(c => new Foo(null)));
			rezolver.Builder.Register(ConstructorTarget.Auto<Bar>(), path: "name1.name2");

			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1.name2");
			Assert.IsNull(result.Foo.Name);
		}
	}
}
