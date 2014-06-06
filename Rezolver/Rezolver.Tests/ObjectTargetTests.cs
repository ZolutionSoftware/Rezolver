using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Rezolver.Tests
{
	[TestClass]
	public class ObjectTargetTests
	{
		[TestMethod]
		public void ShouldWrapNull()
		{
			IRezolveTarget target = new ObjectTarget(null);
			Assert.IsNull(target.GetObject());
		}

		[TestMethod]
		public void ShouldWrapNonNull()
		{
			IRezolveTarget target = new ObjectTarget("Hello world");
			Assert.AreEqual("Hello world", target.GetObject());
		}

		[TestMethod]
		public void ShouldWrapNullableWithNonNullable()
		{
			IRezolveTarget target = new ObjectTarget(1, typeof(int?));
			Assert.IsTrue(target.SupportsType(typeof(int?)));
			Assert.AreEqual((int?)1, target.GetObject());

		}

		[TestMethod]
		public void ShouldAllowAnyBaseAsTargetType()
		{
			IRezolveTarget target = new ObjectTarget("hello world", typeof(IEnumerable<char>));
			IRezolveTarget target2 = new ObjectTarget("hello world", typeof(object));

			string expected = "hello world";
			Assert.AreEqual(expected, target.GetObject());
			Assert.AreEqual(expected, target2.GetObject());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotAllowIncorrectDeclaredType()
		{
			IRezolveTarget target = new ObjectTarget("Hello world", typeof(int));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldRequireTypeParamInSupportsType()
		{
			IRezolveTarget target = new ObjectTarget("Hello world");
			target.SupportsType(null);
		}

		[TestMethod]
		public void Extension_ShouldDeriveDeclaredType()
		{
			IRezolveTarget target = (1).AsObjectTarget();

			Assert.AreEqual(typeof(int), target.DeclaredType);
			Assert.IsTrue(target.SupportsType(typeof(int)));
		}

		[TestMethod]
		public void Extension_ShouldAllowBaseType()
		{
			IRezolveTarget target = (1).AsObjectTarget(typeof(object));
			Assert.AreEqual(typeof(object), target.DeclaredType);
			Assert.AreEqual((object)1, target.GetObject());
		}
	}
}
