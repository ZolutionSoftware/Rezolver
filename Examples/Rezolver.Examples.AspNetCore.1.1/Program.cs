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
	// <example>
	public class Program
    {
        public static void Main(string[] args)
        {
			var host = new WebHostBuilder()
                .UseKestrel()				
				.UseRezolver()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
	// </example>
}
