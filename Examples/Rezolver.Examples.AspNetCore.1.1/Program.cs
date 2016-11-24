using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Rezolver.Logging;
using Microsoft.Extensions.Logging;

namespace Rezolver.Examples.AspNetCore._1._1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
				.ConfigureLogging(f => {
					f.AddDebug(); //required if you want to see registration messages
				})
				.UseRezolver(/*opts =>
				{
					opts.CallEventsMessageType = MessageType.Debug;
					opts.LoggerName = "Rezolver Demo";
				}*/)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
