using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class NonCriticalTests
	{
		[Fact]
		public void NothingTest()
		{
			Console.WriteLine($"3302 Date in ticks: {new DateTime(3302, 2, 18, 14, 52, 9).Ticks}");
		}
	}
}
