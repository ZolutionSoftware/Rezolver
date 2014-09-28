using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;

namespace Rezolver.Examples.Mvc.Areas.Area1.Controllers
{
    public class HomeController : Controller
    {
			private const string DefaultMessage = "Hello Area Default";
			private MessagesModel _messagesModel = new MessagesModel() { MainMessage = DefaultMessage, OriginalRezolveName = "NotResolved" };
			public HomeController() { }
			public HomeController(MessagesModel messagesModel)
			{
				_messagesModel = messagesModel;
			}
        // GET: Area1/Home
        public ActionResult Index()
        {
					ViewBag.Message = _messagesModel.MainMessage;
					ViewBag.OriginalRezolveName = _messagesModel.OriginalRezolveName;
          return View();
        }
    }
}