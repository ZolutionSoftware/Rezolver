using System;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class DynamicRezolverTests : TestsBase
	{
		[TestMethod]
		public void ShouldRezolveFromDynamicContainer()
		{
			//this is using constructorTarget with a prescribed new expression
			var builderMock = new Mock<IRezolverBuilder>();
			//scope1Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(new RezolvedTarget(typeof(int)));

			//the thing being that the underlying Builder does not know how too resolve an integer without
			//being passed a dynamic container at call-time.

			var rezolverMock = new Mock<IRezolver>();
			rezolverMock.Setup(c => c.CanResolve(It.Is((RezolveContext r) => r.RequestedType == typeof(int)))).Returns(true);
			int expected = -1;
			rezolverMock.Setup(c => c.Resolve(It.Is((RezolveContext r) => r.RequestedType == typeof(int))))
				.Returns(expected);
			DefaultRezolver rezolver = new DefaultRezolver(builderMock.Object);

			Assert.AreEqual(expected, rezolver.Resolve(typeof(int)));
		}

		public class Bug_Dependency
		{

		}

		public class Bug_Dependant
		{
			public Bug_Dependency Dependency { get; private set; }
			public Bug_Dependant(Bug_Dependency dependency)
			{
				Dependency = dependency;
			}
		}

		public class Rezolver1 : DefaultRezolver
		{
			public Rezolver1(RezolverBuilder rezolverBuilder, IRezolveTargetCompiler compiler) : base(rezolverBuilder, compiler) { }

			public override string ToString()
			{
				return "Rezolver1";
			}
		}

		public class Rezolver2 : DefaultRezolver
		{
			public Rezolver2(RezolverBuilder rezolverBuilder, IRezolveTargetCompiler compiler) : base(rezolverBuilder, compiler) { }

			public override string ToString()
			{
				return "Rezolver2";
			}
		}

		[TestMethod]
		public void Bug_DynamicRezolverFallingBackToDefaultOnConstructorParameter()
		{
			//var assemblyBuilder = AssemblyRezolveTargetCompiler.CreateAssemblyBuilder(System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
			//var compiler = new AssemblyRezolveTargetCompiler(assemblyBuilder);
			////new RezolveTargetDelegateCompiler()

			////the scenario is a type defined in a dynamically passed rezolver which requires
			////a type rezolved from the root rezolver.

			////just using the rezolver as a builder also
			//var rezolver1 = new Rezolver1(new RezolverBuilder(), compiler);
			//var rezolver2 = new Rezolver2(new RezolverBuilder(), compiler);

			//rezolver2.Register(ConstructorTarget.Auto<Bug_Dependency>());
			//rezolver1.Register(ConstructorTarget.Auto<Bug_Dependant>());

			////var target = rezolver1.FetchCompiled(typeof(Bug_Dependant), null);

			////the rezolvecontext that is passed to the constructor target and then passed
			////on to any rezolvedtarget causes a stac overflow.  Not sure exactly how 
			////to prevent this...
			//try
			//{
			//	var result = rezolver2.Resolve(typeof(Bug_Dependant), rezolver1);
			//	Assert.IsNotNull(result);
			//	Assert.IsInstanceOfType(result, typeof(Bug_Dependant));
			//	Assert.IsNotNull(((Bug_Dependant)result).Dependency);
			//}
			//finally
			//{
			//	string assemblyFileName = compiler.AssemblyBuilder.GetName().Name + ".dll";
			//	compiler.AssemblyBuilder.Save(assemblyFileName);
			//	Console.WriteLine("Wrote {0}", assemblyFileName);
			//}
		}
	}
}
