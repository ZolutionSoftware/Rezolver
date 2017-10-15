using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Rezolver.Options;

namespace Rezolver.Examples.AspNetCore._2._0
{
    // <example>
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseRezolver()
                .UseStartup<Startup>()
                .Build();

        // NOTE - you can also pass a configuration callback to UseRezolver(), e.g:

        /*  
         *  .UseRezolver(o =>
         *  {
         *      // Example of disabling contravariance
         *      o.TargetContainerConfig.ConfigureOption<EnableContravariance>(false);
         *
         *      // you can also add configuration to the container configuration via 
         *      // the ContainerConfig property (e.g. change compilers).
         *  })
         */
    }
    // </example>
}
