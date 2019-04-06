using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Docs.Models;
using Rezolver.Targets;

namespace Rezolver.Docs.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string pathInfo)
        {
            return Redirect(Url.Content("/developers"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode, [FromServices]ICompositeViewEngine viewEngine)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var model = new ErrorViewModel()
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode ?? 999,
                OriginalPath = feature?.OriginalPath
            };

            string viewName = null;

            if(statusCode != null)
            {
                viewName = viewEngine.FindView(ControllerContext, $"Error{statusCode.Value}", true)?.ViewName;
            }

            return View(viewName, model);
        }
    }
}
