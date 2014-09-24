using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rezolver.Examples.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private string _message;
		public HomeController(string message)
		{
			_message = message;
		}
		public ActionResult Index()
		{
			return View((object)_message);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}