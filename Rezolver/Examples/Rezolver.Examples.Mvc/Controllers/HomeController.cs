using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rezolver.Examples.Mvc.Models;

namespace Rezolver.Examples.Mvc.Controllers
{
	public class HomeController : Controller
	{
		private MessagesModel _messagesModel;
		public HomeController()
		{
			_messagesModel = new MessagesModel() { MainMessage = "Default Message from Code", OriginalRezolveName = null };
		}
		public HomeController(MessagesModel messagesModel)
		{
			_messagesModel = messagesModel;
		}
		public ActionResult Index()
		{
			ViewBag.Message = _messagesModel.MainMessage;
			ViewBag.OriginalResolveName = _messagesModel.OriginalRezolveName;

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = _messagesModel.MainMessage;
			ViewBag.OriginalResolveName = _messagesModel.OriginalRezolveName;

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = _messagesModel.MainMessage;
			ViewBag.OriginalResolveName = _messagesModel.OriginalRezolveName;

			return View();
		}
	}
}