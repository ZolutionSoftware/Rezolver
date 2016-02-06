using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class ListTargetTests : TestsBase
	{
		[Fact]
		public void ShouldConstructList()
		{
			//enumerable of object targets
			var elements = Enumerable.Range(0, 3).Select(i => string.Format("Item{0}", i).AsObjectTarget());
			var target = new ListTarget(typeof(string), elements);
			var result = GetValueFromTarget<List<string>>(target);
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
			Assert.True(Enumerable.Range(0, 3).Select(i => string.Format("Item{0}", i)).SequenceEqual(result));
		}

		[Fact]
		public void ShouldConstructArray()
		{
			var elements = Enumerable.Range(0, 3).Select(i => i.AsObjectTarget());
			var target = new ListTarget(typeof(int), elements, true);
			var result = GetValueFromTarget<int[]>(target);
			Assert.NotNull(result);
			Assert.Equal(3, result.Length);
			Assert.True(Enumerable.Range(0, 3).SequenceEqual(result));
		}

		[Fact]
		public void ArrayAndListShouldReturnIEnumerable()
		{
			//just checking that the target complies with the standard type checking
			var elements = Enumerable.Range(0, 3).Select(i => i.AsObjectTarget());
			var listTarget = new ListTarget(typeof(int), elements);
			var arrayTarget = new ListTarget(typeof(int), elements, true);

			var resultList = GetValueFromTarget<IEnumerable<int>>(listTarget);
			var resultArray = GetValueFromTarget<IEnumerable<int>>(arrayTarget);
			Assert.NotNull(resultList);
			Assert.NotNull(resultArray);
			Assert.IsType<List<int>>(resultList);
			Assert.IsType<int[]>(resultArray);
			Assert.True(Enumerable.Range(0, 3).SequenceEqual(resultList));
			Assert.True(Enumerable.Range(0, 3).SequenceEqual(resultArray));
		}
	}
}
