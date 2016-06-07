using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

#if PORTABLE
namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// TODO: Might be able to remove this class once the transition away from the PCL project is complete.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class CallerMemberNameAttribute : Attribute
	{
	}
}
#endif

namespace Rezolver.Diagnostics
{
	/// <summary>
	/// Interface for an object that tracks function calls to Rezolver components.  There are specialised versions of
	/// Rezolver types that support tracking.  See the remarks section for more.
	/// </summary>
	/// <remarks>
	/// An implementation of this interface is expected to be both thread-aware and thread-safe, in that calls that are being
	/// tracked should be expected to be coming from multiple threads.
	/// 
	/// To track calls to a <see cref="Container"/>, swap it with <see cref="TrackedDefaultRezolver"/>
	/// To track calls to a <see cref="ScopedContainer"/>, swap it with <see cref="TrackedLifetimeScopeResolver"/>
	/// To track calls to a <see cref="OverridingScopedContainer"/>, swap it with <see cref="TrackedCombinedLifetimeScopeRezolver"/>
	/// To track calls to a <see cref="Builder"/>, swap it with <see cref="TrackedRezolverBuilder"/>
	/// 
	/// Each of these types requires a reference to this interface that will receive tracking calls for each suitable method or operation.
	/// </remarks>
	public interface ICallTracker
	{
		/// <summary>
		/// Grabs the current snapshot of all root (non-child) calls that have completed, in chronological order
		/// 
		/// By definition, any child calls of these will have been completed also.
		/// </summary>
		/// <returns></returns>
		TrackedCallGraph GetCompletedCalls();
		/// <summary>
		/// Retrieves a complete or in-progress call by its ID.
		/// </summary>
		/// <param name="callID"></param>
		/// <returns></returns>
		TrackedCall GetCall(int callID);
		/// <summary>
		/// Indicates that a function call is commencing on the <paramref name="callee"/> object with the supplied arguments.
		/// The name of the method should be supplied.  The function returns a unique identifier for this function call in order to 
		/// collate any further operations against this method call.
		/// </summary>
		/// <param name="callee"></param>
		/// <param name="arguments"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		int CallStart(object callee, object arguments, [CallerMemberName]string method = null);

		/// <summary>
		/// Records the termination of the function call that was originally given the id <paramref name="callID"/>, along with the 
		/// result that was returned from the function.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="callID"></param>
		/// <param name="result"></param>
		void CallResult<TResult>(int callID, TResult result);
		/// <summary>
		/// Records the termination of function call that was originally given the id <paramref name="callID"/> - this is used
		/// for functions without a return type.
		/// </summary>
		/// <param name="callID"></param>
		void CallEnd(int callID);
		/// <summary>
		/// Records an exception occurring during the execution of the function call that was originally given the id <paramref name="callID"/>.
		/// </summary>
		/// <param name="callID"></param>
		/// <param name="ex"></param>
		void Exception(int callID, Exception ex);
		/// <summary>
		/// Used to log message strings to the logger for a call.
		/// </summary>
		/// <param name="callID"></param>
		/// <param name="message"></param>
		void Message(int callID, string message);
	}

	public static class ICallTrackerExtensions
	{
		public static TResult TrackCall<TResult>(this ICallTracker tracker, object callee, Func<TResult> call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = tracker.CallStart(callee, arguments, methodName);

			TResult result;

			try
			{
				result = call();
			}
			catch (Exception ex)
			{
				tracker.Exception(reqId, ex);
				throw;
			}

			tracker.CallResult(reqId, result);
			return result;
		}

		public static void TrackCall(this ICallTracker tracker, object callee, Action call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = tracker.CallStart(callee, arguments, methodName);

			try
			{
				call();
			}
			catch (Exception ex)
			{
				tracker.Exception(reqId, ex);
				throw;
			}

			tracker.CallEnd(reqId);
		}
	}
}
