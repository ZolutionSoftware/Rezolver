using System.Web.Mvc;

namespace Rezolver.Examples.Mvc.Areas.Area1
{
	public class Area1AreaRegistration : RezolvingAreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Area1";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context, IRezolver rezolver)
		{
			if(rezolver != null)
			{
				rezolver.RegisterObject("hello area1 rezolver!", path: AreaName);
				rezolver.RegisterType<Controllers.HomeController>(path: AreaName);
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