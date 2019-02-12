using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Examples.GenericHost
{
    class Program
    {
        public static Task Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                })
                .UseRezolver((context, targets) =>
                {
                    // note - this could've been registered in ConfigureServices() too, via
                    // the ServiceCollection.
                    targets.RegisterType<Service1>();
                })
                .RunConsoleAsync();
        }

        public class Service1 : IHostedService
        {
            private readonly ILogger<Service1> logger;

            public Service1(ILogger<Service1> logger)
            {
                this.logger = logger;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                logger.LogInformation($"Started {typeof(Service1)}");
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                logger.LogInformation($"Stopping {typeof(Service1)}");
                return Task.CompletedTask;
            }
        }
    }
}
