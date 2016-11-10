// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Logging
{
	/// <summary>
	/// Interface for an object that tracks function calls to Rezolver components for the purposes of logging.  There are specialised versions of
	/// Rezolver types that support tracking.  See the remarks section for more.
	/// </summary>
	/// <remarks>
	/// An implementation of this interface is expected to be both thread-aware and thread-safe, in that calls that are being
	/// tracked should be expected to be coming from multiple threads.
	/// 
	/// To track calls to a <see cref="Container"/>, swap it with <see cref="TrackedContainer"/>
	/// To track calls to a <see cref="ScopedContainer"/>, swap it with <see cref="TrackedScopeContainer"/>
	/// To track calls to a <see cref="OverridingScopedContainer"/>, swap it with <see cref="TrackedOverridingScopedContainer"/>
	/// To track calls to a <see cref="TargetContainer"/>, swap it with <see cref="TrackedTargetContainer"/>
	/// 
	/// Each of these types requires a reference to this interface that will receive tracking calls for each suitable method or operation.
	/// 
	/// Also, the tracked versions of these types also create tracked versions of any subtypes for which tracked versions exist.
	/// </remarks>
	public interface ICallTracker
	{
		/// <summary>
		/// Gets the message formatter used by this tracker (and its <see cref="TrackedCall"/> instances) to format objects into human-readable strings.
		/// </summary>
		/// <value>The message formatter.</value>
		LoggingFormatterCollection MessageFormatter { get; }
		/// <summary>
		/// Retrieves a call by its ID.
		/// </summary>
		/// <param name="callID">The ID of the call as previously returned by <see cref="CallStart(object, object, string, object)"/></param>
		/// <returns>A reference to the <see cref="TrackedCall"/> identified by the given <paramref name="callID"/> if found, otherwise: <c>null</c></returns>
		/// <remarks>An implementation must return a valid reference if the call is still in-progress.
		/// 
		/// An in-progress call is defined as one for which <see cref="CallStart(object, object, string)"/> has been called, but none of the <see cref="CallEnd(long)"/>, 
		/// <see cref="CallResult{TResult}(long, TResult)"/> or <see cref="Exception(long, Exception)"/> methods have been called with the same Call ID.
		/// 
		/// An implementation might also allow you to retrieve completed calls, but is not required to do so.
		/// 
		/// An implementation must return <c>null</c> if the call is not found.</remarks>
		TrackedCall GetCall(long callID);
		/// <summary>
		/// Indicates that a function call is commencing on the <paramref name="callee" /> object with the supplied arguments.
		/// The name of the method should be supplied.  The function returns a unique identifier for this function call in order to
		/// collate any further operations against this method call.
		/// </summary>
		/// <param name="callee">The object on which the method was called.</param>
		/// <param name="arguments">An object whose named properties or fields correspond to a named parameter on the <paramref name="method" /></param>
		/// <param name="method">The name of the method that was called.</param>
		/// <param name="data">Optional.  Additional data to store in the <see cref="TrackedCall"/> that is created for this new call.  The publicly readable
		/// properties and fields of this object will be exploded into a dictionary and made available through the <see cref="TrackedCall.Data"/> dictionary.</param>
		/// <returns>The unique call ID of the new call.  Can be used in the other functions on this interface which accept a call ID.</returns>
		long CallStart(object callee, object arguments, [CallerMemberName]string method = null, object data = null);

		/// <summary>
		/// Records the termination of the function call that was originally given the id <paramref name="callID"/>, along with the 
		/// result that was returned from the function.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="callID"></param>
		/// <param name="result"></param>
		void CallResult<TResult>(long callID, TResult result);
		/// <summary>
		/// Records the termination of function call that was originally given the id <paramref name="callID"/> - this is used
		/// for functions without a return type.
		/// </summary>
		/// <param name="callID"></param>
		void CallEnd(long callID);
		/// <summary>
		/// Records an exception occurring during the execution of the function call that was originally given the id <paramref name="callID"/>.
		/// </summary>
		/// <param name="callID"></param>
		/// <param name="ex"></param>
		void Exception(long callID, Exception ex);
		/// <summary>
		/// Used to format log messages into the call identified by <paramref name="callID"/> (which was previously returned by 
		/// <see cref="CallStart(object, object, string)"/>).
		/// </summary>
		/// <param name="callID">ID of the <see cref="TrackedCall"/> to which the message will be added.</param>
		/// <param name="messageType">The type of message.  In logging-type scenarios, can be used to filter out messages below a certain level.</param>
		/// <param name="messageFormat">A simple string or a format string which can optionally be formatted with the arguments passed in <paramref name="formatArgs"/>.</param>
		/// <param name="formatArgs">Values to be used when formatting <paramref name="messageFormat"/>, if it contains format placeholders.</param>
		TrackedCallMessage Message(long callID, MessageType messageType, string messageFormat, params object[] formatArgs);

		/// <summary>
		/// Like <see cref="Message(long, MessageType, string, object[])"/>, except this adds a message whose text is obtained by formatting an <see cref="IFormattable"/> object.
		/// 
		/// Use this for interpolated strings if you want the <see cref="MessageFormatter"/> to format your messages for you.
		/// </summary>
		/// <param name="callID">The call identifier.</param>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="format">The format.</param>
		/// <remarks>You'll primarily use this overload is if you regularly build messages from interpolated strings.
		/// 
		/// The core implementation of this interface (<see cref="CallTracker"/>) utlises a <see cref="LoggingFormatterCollection"/> object to provide 
		/// advanced formatting functionality for objects into the strings that are ultimately stored on <see cref="TrackedCall"/> objects.
		/// 
		/// If you target this overload with your interpolated string, then this advanced formatting will automatically be used to produce the eventual
		/// message, instead of the default .Net string formatting functionality.
		/// 
		/// In order to target the overload for interpolated strings, either capture it first into an <see cref="IFormattable"/> (or <see cref="FormattableString"/>) 
		/// and pass that, or you can pass the interpolated string as the <paramref name="format"/> by name.  See <see cref="LoggingFormatterCollection.Format(IFormattable)"/>
		/// for more.</remarks>
		TrackedCallMessage Message(long callID, MessageType messageType, IFormattable format);
	}

	public static class ICallTrackerExtensions
	{
		/// <summary>
		/// Tracks the call represented by <paramref name="call"/>, which will receive the Call ID as an argument to allow additional logging.
		/// </summary>
		/// <typeparam name="TResult">The type of the t result.</typeparam>
		/// <param name="tracker">The tracker.</param>
		/// <param name="callee">The callee.</param>
		/// <param name="call">The call.</param>
		/// <param name="arguments">The arguments.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <returns>TResult.</returns>
		public static TResult TrackCall<TResult>(this ICallTracker tracker, object callee, Func<long, TResult> call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = tracker.CallStart(callee, arguments, methodName);

			TResult result;

			try
			{
				result = call(reqId);
			}
			catch (Exception ex)
			{
				tracker.Exception(reqId, ex);
				throw;
			}

			tracker.CallResult(reqId, result);
			return result;
		}

		public static TResult TrackCall<TResult>(this ICallTracker tracker, object callee, Func<TResult> call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			return tracker.TrackCall(callee, callId => call(), arguments, methodName);
		}

		/// <summary>
		/// Tracks the call represented by <see cref="call"/> (which does not have a return type), which will receive the Call ID as an argument to allow 
		/// additional logging.
		/// </summary>
		/// <param name="tracker">The tracker.</param>
		/// <param name="callee">The callee.</param>
		/// <param name="call">The call.</param>
		/// <param name="arguments">The arguments.</param>
		/// <param name="methodName">Name of the method.</param>
		public static void TrackCall(this ICallTracker tracker, object callee, Action<long> call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = tracker.CallStart(callee, arguments, methodName);

			try
			{
				call(reqId);
			}
			catch (Exception ex)
			{
				tracker.Exception(reqId, ex);
				throw;
			}

			tracker.CallEnd(reqId);
		}

		public static void TrackCall(this ICallTracker tracker, object callee, Action call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			tracker.TrackCall(tracker, (callId) => call(), arguments, methodName);
		}
	}
}
