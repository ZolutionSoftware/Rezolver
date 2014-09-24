using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rezolver.Examples.Mvc.Areas.Area1.Controllers
{
    public class HomeController : Controller
    {
			private const string DefaultMessage = "Hello A1rea Default";
			private string _message = DefaultMessage;
			public HomeController() { }
			public HomeController(string message)
			{
				_message = message ?? DefaultMessage; 
			}
        // GET: Area1/Home
        public ActionResult Index()
        {
            return View((object)_message);
        }
    }
}