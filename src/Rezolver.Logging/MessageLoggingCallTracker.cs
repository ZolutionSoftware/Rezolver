using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// This implementation of <see cref="ICallTracker"/> automatically formats and adds messages to <see cref="TrackedCall"/> instances in its implementation of 
	/// <see cref="CallStart(object, object, string)"/>, <see cref="CallEnd(long)"/>, <see cref="CallResult{TResult}(long, TResult)"/>
	/// and <see cref="Exception(long, System.Exception)"/> so that the call's <see cref="TrackedCall.Messages"/> collection contains messages for each.
	/// 
	/// It is a decorator which expects another <see cref="ICallTracker"/> instance to be passed to it on construction, to which all calls are ultimately forwarded.
	/// </summary>
	/// <remarks>This decorator class is useful if you want to enable real-time logging of activity through the use of the 
	/// <see cref="ICallTracker.Message(long, string, MessageType)"/> function only.  Because all the other call events are marked by messages when this tracker is used,
	/// if you simply write out every message that's written, you'll get a real-time log of everything that happens.
	/// 
	/// The class is written as a decorator so that you can add message logging to any <see cref="ICallTracker"/> implementation without having to inherit first.  The
	/// extension method <see cref="ICallTrackerMessageLoggingExtensions.AddMessageLogging(ICallTracker)"/> is a simple way to do this - but you should make sure that you
	/// wrap your tracker inside it; not the other way around.
	/// 
	/// The messages are produced by calling the class' own <see cref="Message(long, string, MessageType)"/> function, which forwards the call on to the
	/// inner ICallTracker instance that is provided to it on constructon.
	/// 
	/// Messages produced from the <see cref="CallStart(object, object, string)"/>, <see cref="CallEnd(long)"/> and <see cref="CallResult{TResult}(long, TResult)"/>
	/// functions are assigned the <see cref="MessageType.Trace"/> message type.  Messages produced by the <see cref="Exception(long, System.Exception)"/> function are assigned
	/// the <see cref="MessageType.Error"/> message type.  Any other messages sent through the <see cref="Message(long, string, MessageType)"/> function retain the message type
	/// they are assigned via the function's parameter.</remarks>
	/// <seealso cref="Rezolver.Logging.ICallTracker" />
	public class MessageLoggingCallTracker : ICallTracker
	{
		private readonly ICallTracker _inner;

		public MessageLoggingCallTracker(ICallTracker inner)
		{
			if (inner == null) throw new ArgumentNullException(nameof(inner));
			_inner = inner;
		}

		public void CallEnd(long callID)
		{
			Message(callID, $"<-#{callID} completed", MessageType.Trace);
			_inner.CallEnd(callID);
		}

		public void CallResult<TResult>(long callID, TResult result)
		{
			Message(callID, $"<-#{callID} completed with result: { result }", MessageType.Trace);
			_inner.CallResult(callID, result);
		}

		public long CallStart(object callee, object arguments, [CallerMemberName] string method = null)
		{
			var callID = _inner.CallStart(callee, arguments, method);
			//fetch the call back so we can interrogate the arguments (they should already have been
			//turned into a dictionary, with each argument named)
			var call = GetCall(callID);
			if(call != null)
				Message(callID, $"->#{call.ID} Method: {call.Method}; Args:({ string.Join(", ", call.Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}")) }) on {call.Callee}", MessageType.Trace);
			return callID;
		}

		public void Exception(long callID, Exception ex)
		{
			Message(callID, $"!-#{callID} Exception of type { ex.GetType() } occurred.  Message: { ex.Message }.  Stack trace: { ex.StackTrace }");
			_inner.Exception(callID, ex);
		}

		public TrackedCall GetCall(long callID)
		{
			return _inner.GetCall(callID);
		}

		public TrackedCallMessage Message(long callID, string message, MessageType messageType = MessageType.Information)
		{
			return _inner.Message(callID, message, messageType);
		}
	}

	public static class ICallTrackerMessageLoggingExtensions
	{
		/// <summary>
		/// Wraps the passed call tracker inside an instance of <see cref="MessageLoggingCallTracker"/> and returns it.
		/// 
		/// Use this to 'inherit' the automatic logging of call start/end/exception events into the messages for <see cref="TrackedCall"/> instances.
		/// </summary>
		/// <param name="tracker">The tracker.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="tracker"/> is null.</exception>
		/// <remarks>If the passed instance is equal to or derived from <see cref="MessageLoggingCallTracker"/>, then the tracker will not
		/// be wrapped again, and is returned as-is.</remarks>
		public static ICallTracker AddMessageLogging(this ICallTracker tracker)
		{
			if (tracker == null) throw new ArgumentNullException(nameof(tracker));
			if (tracker is MessageLoggingCallTracker) return tracker;
			return new MessageLoggingCallTracker(tracker);
		}
	}
}
