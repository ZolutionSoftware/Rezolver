using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class ScopeExamples
	{
		[Fact]
		public void ScopeShouldDisposeTransient()
		{
			var container = new Container();
			container.RegisterType<DisposableType>();
			DisposableType result;
			using(var scope = container.CreateScope())
			{
				result = scope.Resolve<DisposableType>();
				Assert.NotNull(result);
			}

			Assert.True(result.Disposed);
		}

		[Fact]
		public void ScopeShouldCreateMultipleTransients()
		{
			//Demonstrating that implicitly scoped transient objects
			//behave just like non-scoped transient objects.
			var container = new Container();
			container.RegisterType<DisposableType>();
			List<DisposableType> results = new List<DisposableType>();
			using(var scope = container.CreateScope())
			{
				//create 10 objects
				Assert.All(Enumerable.Range(0, 10).Select(
					i => scope.Resolve<DisposableType>()),
					result => {
						Assert.NotNull(result);
						//object must be unique
						Assert.DoesNotContain(result, results);
						results.Add(result);
					});
			}

			//scope should dispose all objects
			Assert.All(results, result => Assert.True(result.Disposed));
		}
	}
}
