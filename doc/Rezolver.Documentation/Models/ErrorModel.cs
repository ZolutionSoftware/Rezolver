using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rezolver.Documentation.Models
{
    public class ErrorModel
    {
        private readonly ControllerContext _context;
        public int? StatusCode { get; }
        public Uri AbsoluteUriRequested { get; }
        public string RelativePathRequested
        {
            get
            {
                return AbsoluteUriRequested?.AbsolutePath ?? "<unknown path>";
            }
        }
        public ErrorModel(ControllerContext context)
        {
            _context = context;

            var errorAndPath = (context.HttpContext.Request.Url.Query ?? "").TrimStart('?').Split(';');
            if (errorAndPath.Length >= 2)
            {
                int status = -1;
                if (int.TryParse(errorAndPath[0], out status))
                    StatusCode = status;
                Uri uri;
                if (Uri.TryCreate(errorAndPath[1], UriKind.Absolute, out uri))
                    AbsoluteUriRequested = uri;
            }
        }
    }
}