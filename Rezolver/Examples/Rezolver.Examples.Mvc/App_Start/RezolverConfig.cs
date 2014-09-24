using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

[assembly: PreApplicationStartMethod(typeof(Rezolver.Examples.Mvc.App_Start.RezolverConfig), "Start")]

namespace Rezolver.Examples.Mvc.App_Start
{
	public static class RezolverConfig
	{
		public static void Start()
		{
			RezolveTargetCompiler.Default = new AssemblyRezolveTargetCompiler();
			var resolver = new LoggingRezolver();

			resolver.Register("Hello rezolver!".AsObjectTarget());
			resolver.Register(ConstructorTarget.Auto<Rezolver.Examples.Mvc.Controllers.HomeController>());

			resolver.EventLogged += resolver_EventLogged;
			DependencyResolver.SetResolver(new RezolverDependencyResolver(resolver));
		}

		static void resolver_EventLogged(object sender, LoggingRezolver.LogEventArgs e)
		{
			Debug.WriteLine("Rezolver: " + e.Message);
		}
	}
}