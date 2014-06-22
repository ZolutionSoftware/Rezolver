using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolveTargetAdapterTests
	{
		public class Foo
		{
			internal Foo()
			{
			}
		}
		[TestMethod]
		public void ShouldAdaptToObjectTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget(Expression.Constant(0)), typeof(ObjectTarget));
		}

		[TestMethod]
		public void ShouldAdaptToConstructorTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget(Expression.New(typeof(Foo))), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget(() => 0), typeof(ObjectTarget));

		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget(() => new Foo()), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCall()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.ConvertToTarget((scope) => scope.Rezolve<int>()), typeof(RezolvedTarget));
		}
	}
}
