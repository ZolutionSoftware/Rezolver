using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public static class RezolverHostBuilderExtensions
    {
        public static IHostBuilder UseRezolver(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseServiceProviderFactory(new RezolverServiceProviderFactory());
        }
    }
}
