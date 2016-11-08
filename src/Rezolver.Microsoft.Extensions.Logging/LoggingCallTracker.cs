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
	public class LoggingCallTracker : CallTracker
	{
		private ILogger _logger;
		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingCallTracker"/> class.
		/// </summary>
		/// <param name="logger">The logger to which messages will be written.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public LoggingCallTracker(ILogger logger, bool retainCompletedCalls = false, MessageType? callEventsMessageType = MessageType.Trace)
			: base(retainCompletedCalls: retainCompletedCalls, callEventsMessageType: callEventsMessageType)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			_logger = logger;
		}

		/// <summary>
		/// Adds the message to the passed call and writes it to the ILogger that was passed to this instance on construction.
		/// </summary>
		/// <param name="call">The call to which the message is to be added.</param>
		/// <param name="message">The message string.</param>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>The <see cref="TrackedCallMessage" /> which was created for the message.</returns>
		protected override TrackedCallMessage AddMessageToCall(TrackedCall call, string message, MessageType messageType)
		{
			var msg = base.AddMessageToCall(call, message, messageType);
			var logMsg = $"(#{ call.ID }) { message }";
			switch (messageType)
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
			return msg;
		}
	}
}
