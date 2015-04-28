using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;

namespace Rezolver.Examples.Mvc.App_Start
{
	public static partial class RezolverConfig
	{
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
			LoggingRezolver loggingRezolver = new LoggingRezolver();

			loggingRezolver.EventLogged += loggingRezolver_EventLogged;

			//this resolver logs nearly all calls to the debug output
			rezolver = loggingRezolver;
#else
			rezolver = new DefaultRezolver();
#endif

			ConfigureRezolver(rezolver);

			return rezolver;
		}

#if DEBUG
		static void loggingRezolver_EventLogged(object sender, LoggingRezolver.LogEventArgs e)
		{
			Debug.WriteLine("Rezolver: " + e.Message, "Rezolver");
		}
#endif

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