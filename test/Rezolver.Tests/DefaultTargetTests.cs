using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DefaultTargetTests : TestsBase
	{
		[Fact]
		public void ShouldReturnDefaultInt()
		{
			DefaultTarget target = new DefaultTarget(typeof(int));
			Assert.Equal(default(int), GetValueFromTarget(target));

		}

		[Fact]
		public void ShouldReturnDefaultNullableInt()
		{
			DefaultTarget target = new DefaultTarget(typeof(int?));
			Assert.Equal((int?)null, GetValueFromTarget(target));

		}

		[Fact]
		public void ShouldReturnDefaultReferenceType_String()
		{
			DefaultTarget target = new DefaultTarget(typeof(string));
			Assert.Equal(null, GetValueFromTarget(target));
		}

		[Fact]
		public void ShouldReturnDefaultReferenceType_Generic()
		{
			DefaultTarget target = new DefaultTarget(typeof(List<int>));
			Assert.Equal(null, GetValueFromTarget(target));
		}
	}
}
