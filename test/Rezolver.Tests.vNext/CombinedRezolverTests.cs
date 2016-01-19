using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
	public class CombinedRezolverTests
	{
		[Fact]
		public void ShouldRezolveFromBaseRezolver()
		{
			//demonstrating how you can simply register directly into a rezolver post construction
			var rezolver1 = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			var rezolver2 = new CombinedRezolver(rezolver1, compiler: rezolver1.Compiler);

			int expectedInt = 10;

			rezolver1.Register(expectedInt.AsObjectTarget());


			Assert.Equal(expectedInt, rezolver2.Resolve(typeof(int)));
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

		[Fact]
		public void Bug_DynamicRezolverFallingBackToDefaultOnConstructorParameter()
		{
			var rezolver1 = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			var rezolver2 = new CombinedRezolver(rezolver1, compiler: rezolver1.Compiler);

			rezolver1.Register(ConstructorTarget.Auto<Bug_Dependant>());
			rezolver2.Register(ConstructorTarget.Auto<Bug_Dependency>());

			var result = rezolver2.Resolve(typeof(Bug_Dependant));
			Assert.NotNull(result);
			Assert.IsType<Bug_Dependant>(result);
			Assert.NotNull(((Bug_Dependant)result).Dependency);
		}

		[Fact]
		public void ShouldFallBackToEnumerableTargetInBase()
		{
			//ISSUE 
			var baseResolver = new DefaultRezolver();
			baseResolver.RegisterObject(1);

			var combinedResolver = new CombinedRezolver(baseResolver);
			var result = combinedResolver.Resolve<IEnumerable<int>>();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}
	}
}
