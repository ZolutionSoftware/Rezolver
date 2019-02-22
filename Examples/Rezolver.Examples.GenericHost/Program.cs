using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rezolver.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Examples.GenericHost
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            return new HostBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Service1>();
                    services.AddHostedService<Service2>();
                })
                .UseRezolver((context, targets) =>
                {
                    // internally, the host resolves services as IEnumerable<IHostedService>,
                    // so this decorator will decorate all hosted services.
                    targets.RegisterDecorator<LoggingServiceDecorator, IHostedService>();
                }, options =>
                {
                    // example of configuring global options for this application
                    options.TargetContainerConfig.ConfigureOption<EnableAutoFuncInjection>(true);
                })
                .RunConsoleAsync();
        }

        public class LoggingServiceDecorator : IHostedService
        {
            private readonly ILogger logger;
            private readonly IHostedService inner;
            public LoggingServiceDecorator(ILoggerFactory loggerFactory, IHostedService inner)
            {
                this.inner = inner;
                this.logger = loggerFactory.CreateLogger(inner.GetType());
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                logger.LogInformation("Started service");
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                logger.LogInformation("Stopped service");
                return Task.CompletedTask;
            }
        }

        public class Service1 : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        public class Service2 : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
