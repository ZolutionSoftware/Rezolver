using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class ContainerTests : TestsBase
  {
    //no - not many tests are there.  This type is used in a lot of other tests though, so does coverage
    //via those.  
    [Fact]
    public void ShouldRezolveAnInt()
    {
      var container = CreateADefaultRezolver();
      container.Register(1.AsObjectTarget(), typeof(int));
      var result = container.Resolve(typeof(int));
      Assert.Equal(1, result);
    }
  }
}
