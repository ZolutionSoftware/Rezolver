using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Rezolver interop extension methods for the IServiceCollection class in Microsoft.Extensions.DependencyInjection
	/// </summary>
	public static class IServiceCollectionExtensions
	{
		/// <summary>
		/// Creates a new default <see cref="Container"/> and registers the services in <paramref name="services"/>
		/// as targets.
		/// </summary>
		/// <param name="services">The services to be registered.</param>
		/// <returns>An <see cref="IContainer"/> instance</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="services"/> is null.</exception>
		public static IContainer CreateRezolverContainer(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			var r = new ScopedContainer();
			r.Populate(services);
			return r;
		}

		/// <summary>
		/// Registers services in <paramref name="services"/> as targets in the passed <paramref name="targetContainer" />
		/// </summary>
		/// <param name="services">The services to b registered.</param>
		/// <param name="targetContainer">The target container that is to receive the new registrations.</param>
		/// <exception cref="ArgumentNullException">If either <paramref name="services"/> or <paramref name="targetContainer"/>
		/// are null.</exception>
		/// <remarks>This extension method just uses the <see cref="ITargetContainerExtensions.Populate(ITargetContainer, IServiceCollection)"/>
		/// method also found in this library.</remarks>
		public static void RegisterTargets(this IServiceCollection services, ITargetContainer targetContainer)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));

			targetContainer.Populate(services);
		}
	}
}
