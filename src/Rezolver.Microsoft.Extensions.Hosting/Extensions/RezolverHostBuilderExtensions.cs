using Rezolver;
using Rezolver.Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides extensions for the .Net Core generic host builder which simplify the process of 
    /// using a Rezolver container as the primary DI container for a .Net Core application.
    /// </summary>
    public static class RezolverHostBuilderExtensions
    {
        /// <summary>
        /// Instructs the host builder to use a Rezolver <see cref="Rezolver.ScopedContainer"/> built from
        /// an <see cref="Rezolver.IRootTargetContainer"/> that is populated with the service collection.
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureOptions">A callback which configures the <see cref="RezolverOptions"/>
        /// that contain the <see cref="ITargetContainerConfig"/> and <see cref="IContainerConfig"/> configuration
        /// objects which will be fed to the <see cref="TargetContainer"/> and <see cref="ScopedContainer"/> which
        /// are created.  Use this to preconfigure Rezolver-specific options, such as <see cref="Rezolver.Options.EnableAutoFuncInjection"/>.</param>
        /// <returns>The host builder</returns>
        /// <remarks>With this overload, you will need to call <see cref="ConfigureRezolver(IHostBuilder, Action{HostBuilderContext, IRootTargetContainer})"/>
        /// or 
        /// <see cref="IHostBuilder.ConfigureContainer{TContainerBuilder}(Action{HostBuilderContext, TContainerBuilder})"/>
        /// at least once to add rezolver-specific registrations, with the type <see cref="IRootTargetContainer"/> as the <code>TContainerBuilder</code>.
        /// 
        /// The other overload allows you </remarks>
        public static IHostBuilder UseRezolver(this IHostBuilder hostBuilder, Action<RezolverOptions> configureOptions = null)
        {
            return hostBuilder.UseServiceProviderFactory(new NetCoreServiceProviderFactory(configureOptions));
        }

        /// <summary>
        /// Provides a callback for performing Rezolver-specific registrations on the <see cref="IRootTargetContainer"/> which will be used
        /// to create the container when <see cref="UseRezolver(IHostBuilder, Action{RezolverOptions})"/> has been called.
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">Required. The configuration callback.</param>
        /// <returns>The host builder</returns>
        /// <remarks>This is a wrapper for <see cref="IHostBuilder.ConfigureContainer{TContainerBuilder}(Action{HostBuilderContext, TContainerBuilder})"/>
        /// with <see cref="IRootTargetContainer"/> as the generic type argument.</remarks>
        public static IHostBuilder ConfigureRezolver(this IHostBuilder hostBuilder, Action<HostBuilderContext, IRootTargetContainer> configureContainer)
        {
            return hostBuilder.ConfigureContainer(configureContainer);
        }

        /// <summary>
        /// Provides a callback for performing Rezolver-specific registrations on the <see cref="IRootTargetContainer"/> which will be used
        /// to create the container when <see cref="UseRezolver(IHostBuilder, Action{RezolverOptions})"/> has been called.
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">Required. The configuration callback.</param>
        /// <returns>The host builder</returns>
        /// <remarks>This is a wrapper for <see cref="IHostBuilder.ConfigureContainer{TContainerBuilder}(Action{HostBuilderContext, TContainerBuilder})"/>
        /// with <see cref="IRootTargetContainer"/> as the generic type argument, and the host builder context fixed to null.</remarks>
        public static IHostBuilder ConfigureRezolver(this IHostBuilder hostBuilder, Action<IRootTargetContainer> configureContainer)
        {
            return hostBuilder.ConfigureContainer<IRootTargetContainer>((context, targets) => configureContainer(targets));
        }
    }
}
