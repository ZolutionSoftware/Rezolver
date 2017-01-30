using Rezolver.Targets;
using Rezolver.Tests.TestTypes;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
  public class SingletonTargetTests : TestsBase
  {
    [Fact]
    public void ShouldNotAllowNullInnerTarget()
    {
      Assert.Throws<ArgumentNullException>(() => new SingletonTarget(null));
    }
    [Fact]
    public void ShouldWrapObjectTarget()
    {
      ITarget target = new SingletonTarget(new ObjectTarget(1));
      Assert.Equal(1, GetValueFromTarget(target));
    }

    [Fact]
    public void ShouldOnlyCreateOneInstanceFromFuncTarget()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        ITarget target = new SingletonTarget(new DelegateTarget<InstanceCountingType>(() => new InstanceCountingType()));
        int instanceCount = InstanceCountingType.InstanceCount;
        var i2 = GetValueFromTarget(target);
        int instanceCount2 = InstanceCountingType.InstanceCount;
        var i3 = GetValueFromTarget(target);
        int instanceCount3 = InstanceCountingType.InstanceCount;

        Assert.Equal(instanceCount + 1, instanceCount2);
        Assert.Equal(instanceCount3, instanceCount2);
      }
    }
  }
}
