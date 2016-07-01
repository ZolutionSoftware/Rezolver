using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Rezolver.Documentation
{
	public class RouteConfig
	{

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.RouteExistingFiles = false;
			routes.IgnoreRoute("learn-rezolver/{*pathInfo}");
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
					name: "Default",
					url: "{*pathInfo}",
					defaults: new { controller = "Home", action = "Index", pathInfo = UrlParameter.Optional }
			);
		}
	}
}