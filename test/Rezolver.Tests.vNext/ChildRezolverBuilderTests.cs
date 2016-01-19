using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
    public class ChildRezolverBuilderTests
    {
		[Fact]
		public void MustNotAllowNullParent()
		{
			Assert.Throws<ArgumentNullException>(() => {
				IChildRezolverBuilder builder = new ChildRezolverBuilder(null);
			});
		}

		[Fact]
		public void MustCopyParent()
		{
			var parent = new RezolverBuilder();
			IChildRezolverBuilder childBuilder = new ChildRezolverBuilder(parent);
			Assert.Same(parent, childBuilder.ParentBuilder);
		}

	}
}
