using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;

namespace Rezolver.Tests.Targets
{
	public class SingletonTargetTests : TargetTestsBase
	{
		public SingletonTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullTarget()
		{
			Assert.Throws<ArgumentNullException>(() => new SingletonTarget(null));
		}

		[Fact]
		public void ShouldNotAllowInnerSingletonTarget()
		{
			//actually, this is a little bit silly because it can't check for singletons nested inside 
			//non-singleton targets.  That said, it steps silly mistakes.
			Assert.Throws<ArgumentException>(() => new SingletonTarget(new SingletonTarget(new TestTarget())));
		}

		[Fact]
		public void ShouldAssignTargetFromConstructor()
		{
			var inner = new TestTarget();
			Assert.Same(inner, new SingletonTarget(inner).InnerTarget);
		}

		[Fact]
		public void ShouldInheritInnerTargetsDeclaredType()
		{
			Assert.Equal(typeof(string),
				new SingletonTarget(new TestTarget(typeof(string), useFallBack: false, supportsType: true)).DeclaredType);
		}
	}
}
