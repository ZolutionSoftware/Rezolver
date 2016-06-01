using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class RezolverTargetAdapterTests  : TestsBase
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
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsType<ObjectTarget>(adapter.GetRezolveTarget(Expression.Constant(0)));
		}

		[Fact]
		public void ShouldAdaptToConstructorTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsType<ConstructorTarget>(adapter.GetRezolveTarget(Expression.New(typeof(Foo))));
		}

		[Fact]
		public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsType<ObjectTarget>(adapter.GetRezolveTarget(() => 0));

		}

		[Fact]
		public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsType<ConstructorTarget>(adapter.GetRezolveTarget(() => new Foo()));
		}

		[Fact]
		public void ShouldIdentifyRezolveCall()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsType<RezolvedTarget>(adapter.GetRezolveTarget((helper) => helper.Resolve<int>()));
		}

		[Fact]
		public void ShouldHaveDefaultTargetAdapter()
		{
			Assert.NotNull(RezolveTargetAdapter.Default);
		}

		private class StubAdapter : IRezolveTargetAdapter
		{
			public IRezolveTarget GetRezolveTarget(Expression expression)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public void ShouldAllowSettingDefaultAdapter()
		{
			IRezolveTargetAdapter newAdapter = new StubAdapter();
			IRezolveTargetAdapter previous = RezolveTargetAdapter.Default;
			RezolveTargetAdapter.Default = newAdapter;
			Assert.NotSame(previous, RezolveTargetAdapter.Default);
			Assert.Same(newAdapter, RezolveTargetAdapter.Default);
			RezolveTargetAdapter.Default = previous;
			Assert.Same(previous, RezolveTargetAdapter.Default);
		}

		[Fact]
		public void ShouldNotAllowSettingNullDefaultAdapter()
		{
			Assert.Throws<ArgumentNullException>(() => RezolveTargetAdapter.Default = null);
		}

	}
}
