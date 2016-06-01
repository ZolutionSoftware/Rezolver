using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DefaultRezolverTests : TestsBase
	{
		//no - not many tests are there.  This type is used in a lot of other tests though, so does coverage
		//via those.  
		[Fact]
		public void ShouldRezolveAnInt()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register(1.AsObjectTarget(), typeof(int));
			var result = rezolver.Resolve(typeof(int));
			Assert.Equal(1, result);
		}
	}
}
