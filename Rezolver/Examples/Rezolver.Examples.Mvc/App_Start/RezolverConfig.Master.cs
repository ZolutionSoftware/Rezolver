using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;

[assembly: PreApplicationStartMethod(typeof(Rezolver.Examples.Mvc.App_Start.RezolverConfig), "Start")]

namespace Rezolver.Examples.Mvc.App_Start
{
	public static partial class RezolverConfig
	{
		public static void Start()
		{
			RezolveTargetCompiler.Default = new AssemblyRezolveTargetCompiler();
			var resolver = new LoggingRezolver();
			resolver.RegisterType<RezolvingControllerActivator, IControllerActivator>();

			resolver.RegisterObject("Hello rezolver!");
			resolver.RegisterType<Rezolver.Examples.Mvc.Controllers.HomeController>();
			resolver.RegisterExpression(c => new MessagesModel() { MainMessage = c.Resolve<string>(), OriginalRezolveName = c.Name });

			resolver.EventLogged += resolver_EventLogged;
			DependencyResolver.SetResolver(new RezolverDependencyResolver(resolver));
		}

		static void resolver_EventLogged(object sender, LoggingRezolver.LogEventArgs e)
		{
			Debug.WriteLine("Rezolver: " + e.Message);
		}
	}
}