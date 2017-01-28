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

		[Fact]
		public void ShouldSetProperties()
		{
			var targets = new[] { new TestTarget(typeof(string), supportsType: true) };
			var target1 = new ListTarget(typeof(string), targets, asArray: false);
			var target2 = new ListTarget(typeof(string), targets, asArray: true);

			Assert.Equal(typeof(string), target1.ElementType);
			Assert.NotNull(target1.Items);
			Assert.Collection(target1.Items, targets.Select(t => new Action<ITarget>(tt => Assert.Same(t, tt))).ToArray());
			Assert.False(target1.AsArray);

			//don't need to test the other properties of target1 again: just the AsArray property
			Assert.True(target2.AsArray);
		}

		[Fact]
		public void ShouldSetCorrectDeclaredType()
		{
			var target1 = new ListTarget(typeof(string), new ITarget[0], asArray: false);
			var target2 = new ListTarget(typeof(string), new ITarget[0], asArray: true);

			Assert.Equal(typeof(List<string>), target1.DeclaredType);
			//cheeky (getting bored now... sorry) - make sure the constructor property is set.
			Assert.NotNull(target1.ListConstructor);

			Assert.Equal(typeof(string[]), target2.DeclaredType);
		}
	}
}
