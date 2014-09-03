using System;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class CombinedRezolverTests : TestsBase
	{
		[TestMethod]
		public void ShouldRezolveFromBaseRezolver()
		{
			//demonstrating how you can simply register directly into a rezolver post construction
			var rezolver1 = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			var rezolver2 = new CombinedRezolver(rezolver1, compiler: rezolver1.Compiler);

			int expectedInt = 10;

			rezolver1.Register(expectedInt.AsObjectTarget());


			Assert.AreEqual(expectedInt, rezolver2.Resolve(typeof(int)));
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

		[TestMethod]
		public void Bug_DynamicRezolverFallingBackToDefaultOnConstructorParameter()
		{
			var rezolver1 = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			var rezolver2 = new CombinedRezolver(rezolver1, compiler: rezolver1.Compiler);

			rezolver1.Register(ConstructorTarget.Auto<Bug_Dependant>());
			rezolver2.Register(ConstructorTarget.Auto<Bug_Dependency>());

			var result = rezolver2.Resolve(typeof(Bug_Dependant));
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(Bug_Dependant));
			Assert.IsNotNull(((Bug_Dependant)result).Dependency);
		}
	}
}
