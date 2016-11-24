using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	public class ConfigureRezolverOptions : IConfigureOptions<RezolverOptions>
	{
		private ILoggerFactory _loggerFactory;
		private ObjectFormatterCollection _formatters;

		public ConfigureRezolverOptions(ILoggerFactory loggerFactory, ObjectFormatterCollection formatters = null)
		{
			_loggerFactory = loggerFactory;
			_formatters = formatters;
		}

		public void Configure(RezolverOptions options)
		{
			if(options.Tracking != null && options.Tracking.Enabled)
			{ 
				//options.Tracking.CallTracker = new LoggingCallTracker(_loggerFactory.CreateLogger(options.Tracking.LoggerCategory ?? "Rezolver");
			}
		}
	}
}
