using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Expressions
{
	public class ExpressionBuilderTests
	{
		//public class Foo
		//{
		//	internal Foo()
		//	{
		//	}
		//}
		//[Fact]
		//public void ShouldAdaptToObjectTarget()
		//{
		//	var adapter = new ExpressionTargetBuilder();
		//	//adapter.
		//	new ExpressionTarget(Expression.Constant(0)).;
		//	Assert.IsType<ObjectTarget>(adapter.CreateTarget(Expression.Constant(0)));
		//}

		//[Fact]
		//public void ShouldAdaptToConstructorTarget()
		//{
		//	IExpressionAdapter adapter = ExpressionAdapter.Instance;
		//	Assert.IsType<ConstructorTarget>(adapter.CreateTarget(Expression.New(typeof(Foo))));
		//}

		//[Fact]
		//public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		//{
		//	IExpressionAdapter adapter = ExpressionAdapter.Instance;
		//	var result = adapter.CreateTarget(() => 0);
		//	//could be argued that this test is testing internals (because it relies
		//	//on the fact that a nullary lambda will not be wrapped in a block expression.
		//	Assert.IsType<ObjectTarget>(result);

		//}

		//[Fact]
		//public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		//{
		//	IExpressionAdapter adapter = ExpressionAdapter.Instance;
		//	Assert.IsType<ConstructorTarget>(adapter.CreateTarget(() => new Foo()));
		//}

		//[Fact]
		//public void ShouldIdentifyRezolveCall()
		//{
		//	IExpressionAdapter adapter = ExpressionAdapter.Instance;
		//	Assert.IsType<RezolvedTarget>(adapter.CreateTarget(() => Functions.Resolve<int>()));
		//}

		//[Fact]
		//public void ShouldHaveDefaultTargetAdapter()
		//{
		//	Assert.NotNull(ExpressionAdapter.Default);
		//}

		//private class StubAdapter : IExpressionAdapter
		//{
		//	public ITarget CreateTarget(Expression expression)
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		//[Fact]
		//public void ShouldAllowSettingDefaultAdapter()
		//{
		//	IExpressionAdapter newAdapter = new StubAdapter();
		//	IExpressionAdapter previous = ExpressionAdapter.Default;
		//	ExpressionAdapter.Default = newAdapter;
		//	Assert.NotSame(previous, ExpressionAdapter.Default);
		//	Assert.Same(newAdapter, ExpressionAdapter.Default);
		//	ExpressionAdapter.Default = previous;
		//	Assert.Same(previous, ExpressionAdapter.Default);
		//}

		//[Fact]
		//public void ShouldNotAllowSettingNullDefaultAdapter()
		//{
		//	Assert.Throws<ArgumentNullException>(() => ExpressionAdapter.Default = null);
		//}

	}
}
