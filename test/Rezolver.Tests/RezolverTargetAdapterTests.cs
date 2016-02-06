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
		public void ShouldIdentifyRezolveCallWithStringParameter()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			var result = adapter.GetRezolveTarget((helper) => helper.Resolve<int>("name"));
			Assert.IsType<RezolvedTarget>(result);
			RezolvedTarget result2 = (RezolvedTarget)result;
			Assert.NotNull(result2.Name);
			//because the adapter is intended to identify targets in the expression,
			//a constantexpression is not preserved, but is turned into an ObjectTarget which will later
			//*build* a ConstantExpression.  This level of indirection is required so we can support more
			//advanced voodoo on all aspects of our expressions (such as rezolved string parameters)
			//that scenario is, in fact, tested in the next test :)
			IRezolveTarget nameTarget = result2.Name as IRezolveTarget;
			Assert.NotNull(nameTarget);
			Assert.IsType<ObjectTarget>(nameTarget);
			Assert.Equal("name", GetValueFromTarget(nameTarget));
		}

		[Fact]
		public void ShouldIdentifyRezolveCallWithNestedRezolveCallAsStringParameter()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			//this is a whacky concept - resolving a string to be used as the name
			//for another resolve call.  The point being that an IRezolverAdapter implementation
			//should be recursive in its treatment of expressions.
			var result = adapter.GetRezolveTarget((helper) => helper.Resolve<int>(helper.Resolve<string>()));
			Assert.IsType<RezolvedTarget>(result);
			RezolvedTarget result2 = (RezolvedTarget)result;
			Assert.NotNull(result2.Name);
			Assert.IsType<RezolvedTarget>(result2.Name);
			Assert.Equal(typeof(string), result2.Name.DeclaredType);
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
