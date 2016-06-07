using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ChildBuilderTests
    {
		[Fact]
		public void MustNotAllowNullParent()
		{
			Assert.Throws<ArgumentNullException>(() => {
				IChildTargetContainer builder = new ChildBuilder(null);
			});
		}

		[Fact]
		public void MustCopyParent()
		{
			var parent = new Builder();
			IChildTargetContainer childBuilder = new ChildBuilder(parent);
			Assert.Same(parent, childBuilder.Parent);
		}

	}
}
