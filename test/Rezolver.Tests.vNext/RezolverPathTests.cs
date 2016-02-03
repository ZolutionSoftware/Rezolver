using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
	public class RezolverPathTests
	{
		[Fact]
		public void ShouldCreateFromString()
		{
			RezolverPath path = new RezolverPath("parent");
			Assert.Equal("parent", path.Path);
		}

		[Fact]
		public void ShouldCreateFromStringImplicit()
		{
			RezolverPath path = "parent.child";
			Assert.Equal("parent.child", path.Path);
		}

		[Fact]
		public void ShouldNotAllowNullPath()
		{
			Assert.Throws<ArgumentNullException>(() => new RezolverPath(null));
		}

		[Fact]
		public void ShouldCreateMultiStepPathFromString()
		{
			RezolverPath path = new RezolverPath("parent.child");
			Assert.Equal("parent.child", path.Path);
			Assert.True(path.MoveNext());
			Assert.Equal("parent", path.Current);
			Assert.True(path.MoveNext());
			Assert.Equal("child", path.Current);
			Assert.False(path.MoveNext());
		}

		[Fact]
		public void ShouldAllowPeekingNextPathItem()
		{
			RezolverPath path = new RezolverPath("parent.child.grandchild");
			//before walking starts, the next item should be the first
			Assert.Equal("parent", path.Next);
			path.MoveNext();
			Assert.Equal("child", path.Next);
			path.MoveNext();
			Assert.Equal("grandchild", path.Next);
			Assert.True(path.MoveNext());
			Assert.Null(path.Next);
			Assert.False(path.MoveNext());

		}

		[Theory]
		[InlineData(" ")]
		[InlineData(" a")]
		[InlineData("a ")]
		[InlineData("a. ")]
		[InlineData("a. b")]
		[InlineData("a. .b")]
		public void ShouldNotAllowAnyWhitespace(string path)
		{
			Assert.Throws<ArgumentException>(() => new RezolverPath(path));
		}

		[Fact]
		public void ShouldNotAllowDoubleSeparator()
		{
			Assert.Throws<ArgumentException>(() => new RezolverPath("a..b"));
		}
	}
}
