using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rezolver.Logging;

namespace Rezolver.Tests
{
  public class CallTrackerTests
  {
    [Fact]
    public void ShouldConstructCall()
    {
      CallTracker logger = new CallTracker();

      int callId = logger.CallStart(this, null);
      var logData = logger.GetCall(callId);

      Assert.NotNull(logData);
      Assert.Equal(callId, logData.ID);
    }
  }
}
