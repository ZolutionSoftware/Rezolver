using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;
using System.Runtime.CompilerServices;

namespace Rezolver.Examples.Mvc.App_Start
{
	public static partial class RezolverConfig
	{
		class DebugTraceRezolverLogger : IRezolverLogger
		{
			private readonly CallTrackingRezolverLogger _inner;
			public DebugTraceRezolverLogger(CallTrackingRezolverLogger inner)
			{
				_inner = inner;
			}

			public void CallEnd(int reqId)
			{
				_inner.CallEnd(reqId);
				Debug.WriteLine($"{reqId} ended", "Rezolver");
			}

			public void CallResult<TResult>(int reqId, TResult result)
			{
				_inner.CallResult(reqId, result);
				Debug.WriteLine($"{reqId} ended with result: {(result == null ? "null" : result.ToString())}", "Rezolver");
			}

			public int CallStart(object callee, object arguments, [CallerMemberName] string method = null)
			{
				var callId = _inner.CallStart(callee, arguments, method);
				var loggedCall = _inner.GetCall(callId);

				Debug.WriteLine($"{callId} started.  Target: {loggedCall.Callee}, Method: {loggedCall.Method}. Arguments: { string.Join(", ", loggedCall.Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}")) }", "Rezolver");
				return callId;
			}

			public void Exception(int reqId, Exception ex)
			{
				_inner.Exception(reqId, ex);
			}

			public void Message(string message)
			{
				_inner.Message(message);
				Debug.WriteLine(message, "Rezolver");
			}
		}

		#region create compiler
		public static IRezolveTargetCompiler CreateDefaultRezolveTargetCompiler()
		{
			return new AssemblyRezolveTargetCompiler();
		}
		#endregion

		#region rezolver creation
		public static IRezolver CreateAndConfigureRezolver()
		{
			//this is an initial idea for how to create and initialize your rezolver for the application.
			//Most applications probably don't need change it.
			IRezolver rezolver = null;

			//note that we're not passing a builder to these resolvers on creation - registrations
			//will be done directly into them.
#if DEBUG
			//this resolver logs nearly all calls to the debug output
			rezolver = new LoggingDefaultRezolver(new DebugTraceRezolverLogger(new CallTrackingRezolverLogger()));
#else
			rezolver = new DefaultRezolver();
#endif

			ConfigureRezolver(rezolver);

			return rezolver;
		}

		#endregion

		#region MVC IDependencyResolver instance creation

		public static IDependencyResolver CreateDependencyResolverInstance(IRezolver resolver)
		{
			//called by the RezolverConfig.Start method to get the IDependencyResolver to
			//set as MVCs default resolver.
			return new RezolverDependencyResolver(resolver);
		}

		#endregion

		public static void ConfigureRezolver(IRezolver rezolver)
		{
			rezolver.RegisterType<RezolvingControllerActivator, IControllerActivator>();
			rezolver.RegisterObject("Hello rezolver!");
			rezolver.RegisterType<Rezolver.Examples.Mvc.Controllers.HomeController>();
			rezolver.RegisterExpression(c => new MessagesModel() { MainMessage = c.Resolve<string>(), OriginalRezolveName = c.Name });

		}
	}
}