using System.Web;
using System.Web.Mvc;

namespace Rezolver.Examples.Mvc
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
