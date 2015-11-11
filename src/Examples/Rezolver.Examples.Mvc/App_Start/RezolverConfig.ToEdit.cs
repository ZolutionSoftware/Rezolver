using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;
using System.Runtime.CompilerServices;
using Rezolver.Diagnostics;

namespace Rezolver.Examples.Mvc.App_Start
{
	public static partial class RezolverConfig
	{
		class DebugTraceLoggingTarget : ILoggingTarget
		{
			public void Ended(TrackedCall call)
			{
				Debug.WriteLine($"<-#{call.ID} ended", "Rezolver");
			}

			public void Exception(TrackedCall call)
			{
				Debug.WriteLine($"!#{call.ID} {call.Exception}", "Rezolver");
			}

			public void Message(string message, TrackedCall call)
			{
				Debug.WriteLine($"#{call.ID} {message}");
			}

			public void Result(TrackedCall call)
			{
				Debug.WriteLine($"<-#{call.ID} result: { call.Result }");
			}

			public void Started(TrackedCall call)
			{
				Debug.WriteLine($"->#{call.ID} {call.Method}({ string.Join(", ", call.Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}")) }) on {call.Callee}");
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

			LoggingCallTracker.Default.AddTargets(new DebugTraceLoggingTarget());
			//note that we're not passing a builder to these resolvers on creation - registrations
			//will be done directly into them.
#if DEBUG
			//this resolver logs nearly all calls to the debug output
			rezolver = new TrackedDefaultRezolver(LoggingCallTracker.Default);
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