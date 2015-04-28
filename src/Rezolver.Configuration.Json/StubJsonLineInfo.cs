using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	internal class StubJsonLineInfo : IJsonLineInfo
	{
		internal static readonly StubJsonLineInfo Instance = new StubJsonLineInfo();
		public bool HasLineInfo()
		{
			return false;
		}

		public int LineNumber
		{
			get { throw new NotImplementedException(); }
		}

		public int LinePosition
		{
			get { throw new NotImplementedException(); }
		}
	}
}
