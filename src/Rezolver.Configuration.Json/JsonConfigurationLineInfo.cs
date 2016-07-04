// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	internal class JsonConfigurationLineInfo : IConfigurationLineInfo
	{
		internal JsonConfigurationLineInfo(IJsonLineInfo start, IJsonLineInfo end = null)
		{
			StartLineNo = start.LineNumber;
			StartLinePos = start.LinePosition;
			if(end != null)
			{
				EndLineNo = end.LineNumber;
				EndLinePos = end.LinePosition;
			}
		}
		public int? StartLineNo
		{
			get;
			private set;
		}

		public int? StartLinePos
		{
			get;
			private set;
		}

		public int? EndLineNo
		{
			get;
			private set;
		}

		public int? EndLinePos
		{
			get;
			private set;
		}
	}
}
