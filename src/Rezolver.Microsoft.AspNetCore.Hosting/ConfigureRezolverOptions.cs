// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class ConfigureRezolverOptions : IConfigureOptions<RezolverOptions>
	{
		//private ILoggerFactory _loggerFactory;
		//private ObjectFormatterCollection _formatters;

		//public ConfigureRezolverOptions(ILoggerFactory loggerFactory, ObjectFormatterCollection formatters = null)
		//{
		//	_loggerFactory = loggerFactory;
		//	_formatters = formatters;
		//}

		public void Configure(RezolverOptions options)
		{
			//if (options.Tracking != null && options.Tracking.Enabled)
			//{
			//	//options.Tracking.CallTracker = new LoggingCallTracker(_loggerFactory.CreateLogger(options.Tracking.LoggerCategory ?? "Rezolver");
			//}
		}
	}
}
