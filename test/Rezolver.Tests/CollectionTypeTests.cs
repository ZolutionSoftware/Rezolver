using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class CollectionTypeTests
    {
        //this tests the internal code used to probe for collection types for member binding purposes.

        //[Fact]
        public void ShouldFindLinkedListAddMethod()
        {


            // Arrange
            var linkedListType = typeof(LinkedList<int>);

            // Act
            var info = linkedListType.GetBindableCollectionTypeInfo();

            // Assert
            Assert.NotNull(info);
            Assert.Equal(typeof(int), info.ElementType);
            Assert.NotNull(info.AddMethod);
        }
    }
}
