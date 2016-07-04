using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class IEnumerableResolveTests
  {
    public class MissingService
    {

    }

    [Fact]
    public void ShouldResolveEmptyEnumerableOfMissingService()
    {
      Container rezolver = new Container();
      var result = rezolver.Resolve<IEnumerable<MissingService>>();
    }
  }
}
