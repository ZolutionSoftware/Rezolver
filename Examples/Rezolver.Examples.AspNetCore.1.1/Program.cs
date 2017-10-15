using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Rezolver.Examples.AspNetCore._1._1
{
    // <example>
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                //add the Rezolver container to the host builder
                .UseRezolver()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
    // </example>
}
