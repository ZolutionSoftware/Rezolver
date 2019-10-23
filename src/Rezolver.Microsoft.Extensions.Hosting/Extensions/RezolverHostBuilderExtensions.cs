// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver;
using System;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides extensions for the .Net Core generic host builder to simplify the process of 
    /// using Rezolver as the DI container for a .Net Core application.
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
        /// The <see cref="UseRezolver(IHostBuilder, Action{HostBuilderContext, IRootTargetContainer}, Action{RezolverOptions})"/>
        /// and <see cref="UseRezolver(IHostBuilder, Action{IRootTargetContainer}, Action{RezolverOptions})"/> overloads, on the other hand,
        /// allow you to provide one initial callback in which you can perform Rezolver-specific operations.</remarks>
        public static IHostBuilder UseRezolver(
            this IHostBuilder hostBuilder,
            Action<RezolverOptions> configureOptions = null)
        {
            return hostBuilder.UseServiceProviderFactory(new RezolverServiceProviderFactory(configureOptions));
        }

        /// <summary>
        /// Instructs the host builder to use a Rezolver <see cref="Rezolver.ScopedContainer"/> built from
        /// an <see cref="Rezolver.IRootTargetContainer"/> that is populated with the service collection.
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">A callback which will perform Rezolver-specific registrations/configuration
        /// when the service provider is finally built.  Additional callbacks can be added with a call to
        /// <see cref="ConfigureRezolver(IHostBuilder, Action{HostBuilderContext, IRootTargetContainer})"/></param>
        /// <param name="configureOptions">Optional. A callback which configures the <see cref="RezolverOptions"/>
        /// that contain the <see cref="ITargetContainerConfig"/> and <see cref="IContainerConfig"/> configuration
        /// objects which will be fed to the <see cref="TargetContainer"/> and <see cref="ScopedContainer"/> which
        /// are created.  Use this to preconfigure Rezolver-specific options, such as <see cref="Rezolver.Options.EnableAutoFuncInjection"/>.</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder UseRezolver(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IRootTargetContainer> configureContainer,
            Action<RezolverOptions> configureOptions = null)
        {
            return hostBuilder
                .UseServiceProviderFactory(new RezolverServiceProviderFactory(configureOptions))
                .ConfigureRezolver(configureContainer);
        }

        /// <summary>
        /// Instructs the host builder to use a Rezolver <see cref="Rezolver.ScopedContainer"/> built from
        /// an <see cref="Rezolver.IRootTargetContainer"/> that is populated with the service collection.
        /// </summary>
        /// <param name="hostBuilder">The host builder</param>
        /// <param name="configureContainer">A callback which will perform Rezolver-specific registrations/configuration
        /// when the service provider is finally built.  Additional callbacks can be added with a call to
        /// <see cref="ConfigureRezolver(IHostBuilder, Action{IRootTargetContainer})"/></param>
        /// <param name="configureOptions">Optional. A callback which configures the <see cref="RezolverOptions"/>
        /// that contain the <see cref="ITargetContainerConfig"/> and <see cref="IContainerConfig"/> configuration
        /// objects which will be fed to the <see cref="TargetContainer"/> and <see cref="ScopedContainer"/> which
        /// are created.  Use this to preconfigure Rezolver-specific options, such as <see cref="Rezolver.Options.EnableAutoFuncInjection"/>.</param>
        /// <returns>The host builder</returns>
        public static IHostBuilder UseRezolver(
            this IHostBuilder hostBuilder,
            Action<IRootTargetContainer> configureContainer,
            Action<RezolverOptions> configureOptions = null)
        {
            return hostBuilder
                .UseServiceProviderFactory(new RezolverServiceProviderFactory(configureOptions))
                .ConfigureRezolver(configureContainer);
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
        public static IHostBuilder ConfigureRezolver(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IRootTargetContainer> configureContainer)
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
        public static IHostBuilder ConfigureRezolver(
            this IHostBuilder hostBuilder,
            Action<IRootTargetContainer> configureContainer)
        {
            return hostBuilder.ConfigureContainer<IRootTargetContainer>((context, targets) => configureContainer(targets));
        }
    }
}
