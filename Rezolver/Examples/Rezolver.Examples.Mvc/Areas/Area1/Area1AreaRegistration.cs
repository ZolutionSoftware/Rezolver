using System.Web.Mvc;

namespace Rezolver.Examples.Mvc.Areas.Area1
{
	public class Area1AreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Area1";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var rezolverDepRezolver = DependencyResolver.Current as RezolverDependencyResolver;
			if(rezolverDepRezolver != null)
			{
				rezolverDepRezolver.Rezolver.RegisterObject("hello area1 rezolver!", path: AreaName);
				rezolverDepRezolver.Rezolver.RegisterType<Controllers.HomeController>(path: AreaName);
			}
			//can register 
			context.MapRoute(
					"Area1_default",
					"Area1/{controller}/{action}/{id}",
					new { action = "Index", id = UrlParameter.Optional },
					new[] { typeof(Area1AreaRegistration).Namespace + ".Controllers" }
			);
		}
	}
}