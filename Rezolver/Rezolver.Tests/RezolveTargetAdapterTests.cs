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
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(Expression.Constant(0)), typeof(ObjectTarget));
		}

		[TestMethod]
		public void ShouldAdaptToConstructorTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(Expression.New(typeof(Foo))), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(() => 0), typeof(ObjectTarget));

		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(() => new Foo()), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCall()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			Assert.IsInstanceOfType(adapter.GetRezolveTarget((scope) => scope.Rezolve<int>()), typeof(RezolvedTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCallWithStringParameter()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			var result = adapter.GetRezolveTarget((scope) => scope.Rezolve<int>("name"));
			Assert.IsInstanceOfType(result, typeof(RezolvedTarget));
			RezolvedTarget result2 = (RezolvedTarget) result;
			Assert.IsNotNull(result2.Name);
			ConstantExpression nameExpr = result2.Name as ConstantExpression;
			Assert.AreEqual("name", nameExpr.Value);
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCallWithNestedRezolveCallAsStringParameter()
		{
			IRezolveTargetAdapter adapter = new RezolveTargetAdapter();
			//this is a whacky concept - resolving a string to be used as the name
			//for another resolve call.  The point being that 
			var result = adapter.GetRezolveTarget((scope) => scope.Rezolve<int>(scope.Rezolve<string>()));
			Assert.IsInstanceOfType(result, typeof(RezolvedTarget));
			RezolvedTarget result2 = (RezolvedTarget)result;
			Assert.IsNotNull(result2.Name);
			RezolveTargetExpression nameExpr = result2.Name as RezolveTargetExpression;

			Assert.AreEqual(typeof(string), nameExpr.Type);
		}

		
	}
}
