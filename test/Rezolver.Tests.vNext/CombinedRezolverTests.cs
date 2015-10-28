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
