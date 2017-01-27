using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
    public class ListTargetTests : TargetTestsBase
    {
		public ListTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => new ListTarget(null, new ITarget[] { }));
			Assert.Throws<ArgumentNullException>(() => new ListTarget(typeof(string), null));
		}

		[Fact]
		public void ShouldNotAllowNullItems()
		{
			Assert.Throws<ArgumentException>(() => new ListTarget(typeof(string), new ITarget[] { null }));
			Assert.Throws<ArgumentException>(() => new ListTarget(typeof(string), new ITarget[] { new TestTarget(typeof(string), supportsType: true), null }));
		}

		[Fact]
		public void ShouldNotAllowItemsWhichDontSupportElementType()
		{
			Assert.Throws<ArgumentException>(() => new ListTarget(typeof(string), new[] { new TestTarget(supportsType: false) }));
			Assert.Throws<ArgumentException>(() => new ListTarget(typeof(string), new[] { new TestTarget(typeof(string), supportsType: false) }));
		}

#error Real functionality tests next
	}
}
