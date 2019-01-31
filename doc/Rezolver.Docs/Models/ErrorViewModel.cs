using System;

namespace Rezolver.Docs.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public int StatusCode { get; set; }
        public string OriginalPath { get; set; }
    }
}