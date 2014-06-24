using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolveTargetAdapterTests : TestsBase
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
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(Expression.Constant(0)), typeof(ObjectTarget));
		}

		[TestMethod]
		public void ShouldAdaptToConstructorTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(Expression.New(typeof(Foo))), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchObjectTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(() => 0), typeof(ObjectTarget));

		}

		[TestMethod]
		public void ShouldWorkOnLambdaBodyToFetchConstructorTarget()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsInstanceOfType(adapter.GetRezolveTarget(() => new Foo()), typeof(ConstructorTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCall()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			Assert.IsInstanceOfType(adapter.GetRezolveTarget((scope) => scope.Rezolve<int>()), typeof(RezolvedTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCallWithStringParameter()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			var result = adapter.GetRezolveTarget((scope) => scope.Rezolve<int>("name"));
			Assert.IsInstanceOfType(result, typeof(RezolvedTarget));
			RezolvedTarget result2 = (RezolvedTarget) result;
			Assert.IsNotNull(result2.Name);
			//oh the irony - because the adapter is intended to identify targets in the expression,
			//a constantexpression is not preserved, but is turned into an ObjectTarget which will later
			//*build* a ConstantExpression.
			IRezolveTarget nameTarget = result2.Name as IRezolveTarget;
			Assert.IsNotNull(nameTarget);
			Assert.IsInstanceOfType(nameTarget, typeof(ObjectTarget));
			Assert.AreEqual("name", GetValueFromTarget(nameTarget));
		}

		[TestMethod]
		public void ShouldIdentifyRezolveCallWithNestedRezolveCallAsStringParameter()
		{
			IRezolveTargetAdapter adapter = RezolveTargetAdapter.Instance;
			//this is a whacky concept - resolving a string to be used as the name
			//for another resolve call.  The point being that an IRezolverAdapter implementation
			//should be recursive in itss treatment of expressions.
			var result = adapter.GetRezolveTarget((scope) => scope.Rezolve<int>(scope.Rezolve<string>()));
			Assert.IsInstanceOfType(result, typeof(RezolvedTarget));
			RezolvedTarget result2 = (RezolvedTarget)result;
			Assert.IsNotNull(result2.Name);
			IRezolveTarget nameTarget = result2.Name as IRezolveTarget;

			Assert.AreEqual(typeof(string), nameTarget.DeclaredType);
		}

		[TestMethod]
		public void ShouldHaveDefaultTargetAdapter()
		{
			Assert.IsNotNull(RezolveTargetAdapter.Default);
		}

		[TestMethod]
		public void ShouldAllowSettingDefaultAdapter()
		{
			IRezolveTargetAdapter moq = Mock.Of<IRezolveTargetAdapter>();
			IRezolveTargetAdapter previous = RezolveTargetAdapter.Default;
			RezolveTargetAdapter.Default = moq;
			Assert.AreNotSame(previous, RezolveTargetAdapter.Default);
			Assert.AreSame(moq, RezolveTargetAdapter.Default);
			RezolveTargetAdapter.Default = previous;
			Assert.AreSame(previous, RezolveTargetAdapter.Default);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowSettingNullDefaultAdapter()
		{
			RezolveTargetAdapter.Default = null;
		}
	}
}
