using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Rezolver
{
	public class RezolvingControllerActivator : IControllerActivator
	{
		//not actually sure this is going to work as I think the controller activator is cached....
		private IRezolver _contextRezolver;

		public RezolvingControllerActivator(IRezolver contextRezolver)
		{
			_contextRezolver = contextRezolver;
		}


		public IController Create(RequestContext requestContext, Type controllerType)
		{
			try
			{
				object controller = null;
				string areaName = (string)requestContext.RouteData.DataTokens["area"];
				string controllerName = (string)requestContext.RouteData.Values["controller"];
				string contextName = areaName != null ? string.Format("{0}.{1}", areaName, controllerName) : controllerName;
				if (!_contextRezolver.TryResolve(controllerType, contextName, out controller))
					return (IController)Activator.CreateInstance(controllerType);
				else
					return (IController)controller;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
						String.Format("An error occurred trying to create an instance of the controller type \"{0}\"",
								controllerType),
						ex);
			}
		}
	}
}
