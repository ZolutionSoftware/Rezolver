using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver.Tests
{
	[TestClass]
	public class ListTargetTests : TestsBase
	{
		[TestMethod]
		public void ShouldConstructList()
		{
			//enumerable of object targets
			var elements = Enumerable.Range(0, 3).Select(i => string.Format("Item{0}", i).AsObjectTarget());
			var target = new ListTarget(typeof(string), elements);
			var result = GetValueFromTarget<List<string>>(target);
			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(Enumerable.Range(0, 3).Select(i => string.Format("Item{0}", i)).SequenceEqual(result));
		}

		[TestMethod]
		public void ShouldConstructArray()
		{
			var elements = Enumerable.Range(0, 3).Select(i => i.AsObjectTarget());
			var target = new ListTarget(typeof(int), elements, true);
			var result = GetValueFromTarget<int[]>(target);
			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Length);
			Assert.IsTrue(Enumerable.Range(0, 3).SequenceEqual(result));
		}

		[TestMethod]
		public void ArrayAndListShouldReturnIEnumerable()
		{
			//just checking that the target complies with the standard type checking
			var elements = Enumerable.Range(0, 3).Select(i => i.AsObjectTarget());
			var listTarget = new ListTarget(typeof(int), elements);
			var arrayTarget = new ListTarget(typeof(int), elements, true);

			var resultList = GetValueFromTarget<IEnumerable<int>>(listTarget);
			var resultArray = GetValueFromTarget<IEnumerable<int>>(arrayTarget);
			Assert.IsNotNull(resultList);
			Assert.IsNotNull(resultArray);
			Assert.IsInstanceOfType(resultList, typeof(List<int>));
			Assert.IsInstanceOfType(resultArray, typeof(int[]));
			Assert.IsTrue(Enumerable.Range(0, 3).SequenceEqual(resultList));
			Assert.IsTrue(Enumerable.Range(0, 3).SequenceEqual(resultArray));
		}
	}
}
