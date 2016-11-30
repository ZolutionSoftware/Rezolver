using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class TargetAdapterTests : TestsBase
	{
		public class Foo
		{
			internal Foo()
			{
			}
		}
		[Fact]
		public void ShouldAdaptToObjectTarget()
		{
			ITargetAdapter adapter = TargetAdapter.Instance;
			Assert.IsType<ObjectTarget>(adapter.CreateTarget(Expression.Constant(0)));
		}

		[Fact]
		public void ShouldAdaptToConstructorTarget()
		{
			ITargetAdapter adapter = TargetAdapter.Instance;
			Assert.IsType<ConstructorTarget>(adapter.CreateTarget(Expression.New(typeof(Foo))));
		}

		[Fact]
		public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		{
			ITargetAdapter adapter = TargetAdapter.Instance;
			var result = adapter.CreateTarget(() => 0);
			//could be argued that this test is testing internals (because it relies
			//on the fact that a nullary lambda will not be wrapped in a block expression.
			Assert.IsType<ObjectTarget>(result);

		}

		[Fact]
		public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		{
			ITargetAdapter adapter = TargetAdapter.Instance;
			Assert.IsType<ConstructorTarget>(adapter.CreateTarget(() => new Foo()));
		}

		[Fact]
		public void ShouldIdentifyRezolveCall()
		{
			ITargetAdapter adapter = TargetAdapter.Instance;
			Assert.IsType<RezolvedTarget>(adapter.CreateTarget(() => Functions.Resolve<int>()));
		}

		[Fact]
		public void ShouldHaveDefaultTargetAdapter()
		{
			Assert.NotNull(TargetAdapter.Default);
		}

		private class StubAdapter : ITargetAdapter
		{
			public ITarget CreateTarget(Expression expression)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public void ShouldAllowSettingDefaultAdapter()
		{
			ITargetAdapter newAdapter = new StubAdapter();
			ITargetAdapter previous = TargetAdapter.Default;
			TargetAdapter.Default = newAdapter;
			Assert.NotSame(previous, TargetAdapter.Default);
			Assert.Same(newAdapter, TargetAdapter.Default);
			TargetAdapter.Default = previous;
			Assert.Same(previous, TargetAdapter.Default);
		}

		[Fact]
		public void ShouldNotAllowSettingNullDefaultAdapter()
		{
			Assert.Throws<ArgumentNullException>(() => TargetAdapter.Default = null);
		}

	}
}
