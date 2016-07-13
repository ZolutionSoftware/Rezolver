using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class ChildTargetContainerTests
  {
    [Fact]
    public void MustNotAllowNullParent()
    {
      Assert.Throws<ArgumentNullException>(() =>
      {
        IChildTargetContainer builder = new ChildTargetContainer(null);
      });
    }

    [Fact]
    public void MustCopyParent()
    {
      var parent = new TargetContainer();
      IChildTargetContainer childBuilder = new ChildTargetContainer(parent);
      Assert.Same(parent, childBuilder.Parent);
    }

  }
}
