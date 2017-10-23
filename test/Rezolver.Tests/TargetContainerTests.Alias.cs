using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class TargetContainer_AliasTests
    {
		[Fact]
		public void ShouldRegisterAlias()
		{
			TargetContainer targets = new TargetContainer();
			targets.RegisterAlias<object, string>();
			Assert.NotNull(targets.Fetch(typeof(object)));
		}
	}
}
