using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Rezolver.Targets;

namespace Rezolver.Tests.Targets
{
	public class RezolvedTargetTests : TargetTestsBase
	{
		public RezolvedTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		//this target represents some complex stuff when compiled, but before then it's
		//pretty darn simple.

		[Fact]
		public void ShouldNotAllowNullType()
		{
			Assert.Throws<ArgumentNullException>(() => new ResolvedTarget(null));
		}

		[Fact]
		public void ShouldRequireFallbackTargetToSupportResolveType()
		{
			Assert.Throws<ArgumentException>(() => new ResolvedTarget(typeof(string), new TestTarget(supportsType: false)));
		}

		[Fact]
		public void ShouldSetAllPropertiesIfProvided()
		{
			var fallback = new TestTarget(supportsType: true);
			var target = new ResolvedTarget(typeof(string), fallback);

			Assert.Equal(typeof(string), target.DeclaredType);
			Assert.Same(fallback, target.FallbackTarget);
		}

		//now the binding tests

		[Fact]
		public void ShouldBindToTargetFromContextContainer()
		{
			var container = new Container();
			container.Register(new TestTarget(typeof(string), useFallBack: false, supportsType: true));

			var target = new ResolvedTarget(typeof(string));
			var context = GetCompileContext(target, container);

			var result = target.Bind(context);
			Assert.NotNull(result);
			Assert.IsType<TestTarget>(result);
		}

		[Fact]
		public void ShouldUseFallbackWhenContextContainerFails()
		{
			var container = new Container();
			var fallback = new TestTarget(typeof(string), useFallBack: false, supportsType: true);

			var target = new ResolvedTarget(typeof(string), fallback);
			var context = GetCompileContext(target, container);

			var result = target.Bind(context);
			Assert.NotNull(result);
			Assert.Same(fallback, result);
		}

		[Fact]
		public void ShouldUseFallbackWhenResolvedTargetUseFallbackIsTrue()
		{
			var container = new Container();
			container.Register(new TestTarget(typeof(string), useFallBack: true, supportsType: true));
			var fallback = new TestTarget(typeof(string), useFallBack: false, supportsType: true);

			var target = new ResolvedTarget(typeof(string), fallback);
			var context = GetCompileContext(target, container);

			var result = target.Bind(context);
			Assert.NotNull(result);
			Assert.Same(fallback, result);
		}

		[Fact]
		public void ShouldBindToNullWhenNoTargetIsResolved()
		{
			var target = new ResolvedTarget(typeof(string));
			var context = GetCompileContext(target);

			Assert.Null(target.Bind(context));
		}
	}
}
