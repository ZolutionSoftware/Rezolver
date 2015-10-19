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

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that receives logging operation calls from Rezolver components.
	/// 
	/// Note - none of the standard rezolver types perform any logging - there are interchangeable alternatives
	/// for the standard rezolver types that can be swapped in in order to receive logging messages.
	/// </summary>
	/// <remarks>
	/// To log from a <see cref="DefaultRezolver"/>, swap it with <see cref="LoggingDefaultRezolver"/>
	/// To log from a <see cref="DefaultLifetimeScopeRezolver"/>, swap it with <see cref="LoggingLifetimeScopeResolver"/>
	/// To log from a <see cref="RezolverBuilder"/>, swap it with <see cref="LoggingRezolverBuilder"/>
	/// 
	/// Each of these types requires a reference to this interface that will receive all the logged information.
	/// </remarks>
	public interface IRezolverLogger
	{
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
		/// Records the termination of the function call that was originally given the id <paramref name="callId"/>, along with the 
		/// result that was returned from the function.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="callId"></param>
		/// <param name="result"></param>
		void CallResult<TResult>(int callId, TResult result);
		/// <summary>
		/// Records the termination of function call that was originally given the id <paramref name="callId"/> - this is used
		/// for functions without a return type.
		/// </summary>
		/// <param name="callId"></param>
		void CallEnd(int callId);
		/// <summary>
		/// Records an exception occurring during the execution of the function call that was originally given the id <paramref name="callId"/>.
		/// </summary>
		/// <param name="callId"></param>
		/// <param name="ex"></param>
		void Exception(int callId, Exception ex);
		/// <summary>
		/// Used to log message strings to the logger.
		/// </summary>
		/// <param name="message"></param>
		void Message(string message);
	}

	public static class IRezolverLoggerExtensions
	{
		/// <summary>
		/// Log a call to a method with 
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="call"></param>
		/// <param name="context"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static TResult LogCallWithResult<TResult>(this IRezolverLogger logger, object callee, Func<TResult> call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = logger.CallStart(callee, arguments, methodName);

			TResult result;

			try
			{
				result = call();
			}
			catch (Exception ex)
			{
				logger.Exception(reqId, ex);
				throw;
			}

			logger.CallResult(reqId, result);
			return result;
		}

		public static void LogCall(this IRezolverLogger logger, object callee, Action call, object arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = logger.CallStart(callee, arguments, methodName);

			try
			{
				call();
			}
			catch (Exception ex)
			{
				logger.Exception(reqId, ex);
				throw;
			}

			logger.CallEnd(reqId);
		}
	}
}
