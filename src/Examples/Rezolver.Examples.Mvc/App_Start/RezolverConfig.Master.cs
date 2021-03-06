﻿using System;
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
			//set the default target compiler for this application so that you don't have to supply one
			//to each rezolver that you create.  The CreateDefaultRezolverTargetCompiler method is found
			//in the RezolverConfig.ToEdit.cs
			RezolveTargetCompiler.Default = CreateDefaultRezolveTargetCompiler();
			var resolver = CreateAndConfigureRezolver();
			DependencyResolver.SetResolver(CreateDependencyResolverInstance(resolver));
		}
	}
}