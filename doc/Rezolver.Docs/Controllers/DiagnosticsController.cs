using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rezolver.Compilation.Expressions;

namespace Rezolver.Docs.Controllers
{
    public class DiagnosticsController : Controller
    {
        public IActionResult Index()
        {
            return View(ExpressionBuilderBase.GetCompileCounts());
        }
    }
}