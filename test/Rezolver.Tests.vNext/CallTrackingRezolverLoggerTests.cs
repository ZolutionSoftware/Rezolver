using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
	public class CallTrackingRezolverLoggerTests
	{
		[Fact]
		public void ShouldConstructCall()
		{
			CallTrackingRezolverLogger logger = new CallTrackingRezolverLogger();

			int callId = logger.CallStart(this, null);
			var logData = logger.GetLoggedCalls();
			
			Assert.NotNull(logData);
			var singleCall = logData.Single();
			Assert.Equal(callId, singleCall.ID);
		}
	}
}
