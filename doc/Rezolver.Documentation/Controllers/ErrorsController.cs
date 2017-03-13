using Rezolver.Documentation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rezolver.Documentation.Controllers
{
    public class ErrorsController : Controller
    {
        private class ErrorDecorator : ActionResult
        {
            ErrorModel _error;
            ActionResult _result;
            public ErrorDecorator(ActionResult result, ErrorModel error)
            {
                _result = result;
                _error = error;
            }
            public override void ExecuteResult(ControllerContext context)
            {
                _result.ExecuteResult(context);
                if(_error != null && _error.StatusCode != null)
                    context.HttpContext.Response.StatusCode = _error.StatusCode.Value;
            }
        }

        private ActionResult Decorate(ActionResult result, ErrorModel error)
        {
            return new ErrorDecorator(result, error);
        }
        public ActionResult DeliberateError()
        {
            throw new InvalidOperationException("This should blow up");
        }
        public ActionResult Error()
        {
            var model = new Models.ErrorModel(ControllerContext);
            if (model.StatusCode != null)
            {
                //try to locate the view for this error
                var result = ViewEngineCollection.FindView(ControllerContext, $"Error{model.StatusCode}", null);
                if (result?.View != null) //if found, execute it, decorating it with our decorator which preserves the original status code.
                    return Decorate(View(result.View, model), model);
            }

            return Decorate(View(model), model);
        }

        // GET: Errors
        public ActionResult Error404()
        {
            var model = new Models.ErrorModel(ControllerContext);
            //query string from IIS comes through as ?[http status];[original url]
            return Decorate(View(model), model);
        }
    }
}