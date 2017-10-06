// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Rezolver interop extension methods for the IServiceCollection class in Microsoft.Extensions.DependencyInjection
	/// </summary>
	public static class RezolverServiceCollectionExtensions
	{
        /// <summary>
        /// Creates a new default <see cref="ScopedContainer"/> and registers the services in <paramref name="services"/>
        /// as targets.
        /// </summary>
        /// <param name="services">The services to be registered.</param>
        /// <returns>An <see cref="IContainer"/> instance</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="services"/> is null.</exception>
        /// <remarks>The <see cref="Rezolver.Options.LazyEnumerables"/> option (is set to <c>false</c> by this method,
        /// because v2.0 of .Net Core 2 seems to expect all enumerables to be eagerly loaded - getting clarification
        /// on this from the team at https://github.com/aspnet/DependencyInjection/issues/589</remarks>
        public static IContainer CreateRezolverContainer(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

            var targetContainer = new TargetContainer(
                TargetContainer.DefaultConfig
                .Clone()
                .ConfigureOption<Rezolver.Options.LazyEnumerables>(false)
                );

			var c = new ScopedContainer(targetContainer);
			c.Populate(services);
			return c;
		}

		/// <summary>
		/// Registers services in <paramref name="services"/> as targets in the passed <paramref name="targetContainer" />
		/// </summary>
		/// <param name="services">The services to be registered.</param>
		/// <param name="targetContainer">The target container that is to receive the new registrations.</param>
		/// <exception cref="ArgumentNullException">If either <paramref name="services"/> or <paramref name="targetContainer"/>
		/// are null.</exception>
		/// <remarks>This extension method just uses the <see cref="MSDIITargetContainerExtensions.Populate(ITargetContainer, IServiceCollection)"/>
		/// method also found in this library.</remarks>
		public static void RegisterTargets(this IServiceCollection services, ITargetContainer targetContainer)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));

			targetContainer.Populate(services);
		}
	}
}
