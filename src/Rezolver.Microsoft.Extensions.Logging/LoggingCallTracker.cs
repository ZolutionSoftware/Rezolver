using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rezolver.Microsoft.Extensions.Logging
{
	/// <summary>
	/// Overrides the <see cref="CallTracker"/> class to provide realtime logging of any messages added to 
	/// calls (represented by <see cref="TrackedCall"/> instances) directly to an ILogger from the Microsoft Logging Extensions.
	/// </summary>
	/// <seealso cref="Rezolver.Logging.CallTracker" />
	/// <remarks>The <see cref="TrackedCall.MessageAdded"/> event is used as the hook by which messages are intercepted and logged to the 
	/// logger.</remarks>
	public class LoggingCallTracker : CallTracker
	{
		private ILogger _logger;
		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingCallTracker"/> class.
		/// </summary>
		/// <param name="logger">The logger to which messages will be written.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public LoggingCallTracker(ILogger logger, bool retainCompletedCalls = false, MessageType? callEventsMessageType = MessageType.Trace, ObjectFormatterCollection messageFormatter = null)
			: base(retainCompletedCalls: retainCompletedCalls, callEventsMessageType: callEventsMessageType, messageFormatter: messageFormatter)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			_logger = logger;
		}

		protected override TrackedCall CreateNewCall(long callID, TrackedCall parent)
		{
			var call = base.CreateNewCall(callID, parent);
			call.MessageAdded += Call_MessageAdded;
			return call;
		}

		private IEnumerable<long> GetCallStackIDs(TrackedCall call)
		{
			while (call != null)
			{
				yield return call.ID;
				call = call.Parent;
			}
		}

		private void Call_MessageAdded(object sender, TrackedCallMessage e)
		{
			var logMsg = $"(#{ string.Join("->", GetCallStackIDs(e.Call).Reverse()) }) { e.Text }";
			switch (e.Type)
			{
				case MessageType.Debug:
					_logger.LogDebug(logMsg);
					break;
				case MessageType.Error:
					_logger.LogError(logMsg);
					break;
				case MessageType.Information:
					_logger.LogInformation(logMsg);
					break;
				case MessageType.Trace:
					_logger.LogTrace(logMsg);
					break;
				case MessageType.Warning:
					_logger.LogWarning(logMsg);
					break;
				default:
					_logger.LogInformation(logMsg);
					break;
			}
		}
	}
}
