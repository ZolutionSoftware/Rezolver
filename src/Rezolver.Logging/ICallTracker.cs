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
		/// Retrieves a call by its ID.
		/// </summary>
		/// <param name="callID">The ID of the call as previously returned by <see cref="CallStart(object, object, string)"/></param>
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
		/// Indicates that a function call is commencing on the <paramref name="callee"/> object with the supplied arguments.
		/// The name of the method should be supplied.  The function returns a unique identifier for this function call in order to 
		/// collate any further operations against this method call.
		/// </summary>
		/// <param name="callee">The object on which the method was called.</param>
		/// <param name="arguments">An object whose named properties or fields correspond to a named parameter on the <paramref name="method"/></param>
		/// <param name="method">The name of the method that was called.</param>
		/// <returns></returns>
		long CallStart(object callee, object arguments, [CallerMemberName]string method = null);

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
		/// Used to log message strings to the logger for a call.
		/// </summary>
		/// <param name="callID"></param>
		/// <param name="message"></param>
		/// <param name="messageType">The type of message in <paramref name="message"/>.  In logging-type scenarios, can be used
		/// to filter out messages below a certain level.</param>
		TrackedCallMessage Message(long callID, string message, MessageType messageType = MessageType.Information);
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
