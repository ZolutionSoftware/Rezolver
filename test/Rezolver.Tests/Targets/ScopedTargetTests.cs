using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;

namespace Rezolver.Tests.Targets
{
    public class ScopedTargetTests : TargetTestsBase
    {
		public ScopedTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullTarget()
		{
			Assert.Throws<ArgumentNullException>(() => new ScopedTarget(null));
		}

		[Fact]
		public void ShouldAssignTargetFromConstructor()
		{
			var inner = new TestTarget();
			Assert.Same(inner, new ScopedTarget(inner).InnerTarget);
		}

		[Fact]
		public void ShouldInheritInnerTargetsDeclaredType()
		{
			Assert.Equal(typeof(string), 
				new ScopedTarget(new TestTarget(typeof(string), useFallBack: false, supportsType: true)).DeclaredType);
		}
	}
}
