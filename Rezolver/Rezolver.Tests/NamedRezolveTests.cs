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
		
		//name-local registrations
		[TestMethod]
		public void NamedObjectShouldResolveLocalDependency1()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path: "name1");
			rezolver.Register(ConstructorTarget.Auto<Bar>(), path: "name1");

			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1");
			Assert.IsNotNull(result.Foo);
			Assert.AreEqual("name1", result.Foo.Name);
		}

		[TestMethod]
		public void NamedObjectShouldResolveLocalDependencyThatOverridesUnnamed()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)));
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo("fixed")), path: "name1");
			rezolver.Register(ConstructorTarget.Auto<Bar>(), path: "name1");

			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1");
			Assert.IsNotNull(result.Foo);
			Assert.AreEqual("fixed", result.Foo.Name);
		}

		[TestMethod]
		public void UnnamedObjectResolveByNameShouldGetNamedDependency()
		{
			//here we register a foo by name but an unnamed bar.  We then
			// create a bar by the name, and it should get the named dependency.
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo("hello named dependency selection")), path: "name1");
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo("should not get called")));
			rezolver.Register(ConstructorTarget.Auto<Bar>());
			//reguest a named instance of Bar, even though we never registered one, only the default.
			//we're using the namme to select the dependency.
			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1");
			Assert.IsNotNull(result.Foo);
			Assert.AreEqual("hello named dependency selection", result.Foo.Name);
			//and then just to prove the point, request an unnamed Bar, and one with a name which doesn't match
			var resultBase = (Bar)rezolver.Resolve(typeof(Bar));
			Assert.IsNotNull(resultBase.Foo);
			Assert.AreEqual("should not get called", resultBase.Foo.Name);
		}

		[TestMethod]
		public void WhenFallingBackShouldUseTheOriginalName()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			//so here we have an object with a name requesting another named dependency which
			//doesn't exist under that object, but sits under the root
			//but notice that when we check the name value that's in the Foo object it'll be the
			//name that was used to resolve the Bar object, because that's the name that's passed from context.
			//the only reason the name would be dependency.name2 would be if, for some reason, it had to late-bind
			//that dependency instead of resolving it at compile time.
			rezolver.Register(ConstructorTarget.For<Foo>(c => new Foo(c.Name)), path: "dependency.name2");
			rezolver.Register(ConstructorTarget.For<Bar>(c => new Bar(c.Resolve<Foo>("dependency.name2"))), path: "name1.name2");
			var result = (Bar)rezolver.Resolve(typeof(Bar), "name1.name2");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Foo);
			Assert.AreEqual("name1.name2", result.Foo.Name);
		}
	}
}
