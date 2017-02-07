using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation
{
	/// <summary>
	/// Controls the system-wide compiler configuration used by default by all standard Rezolver containers.
	/// </summary>
	public static class CompilerConfiguration
	{
		private class NoCompilerConfigurationProvider : ICompilerConfigurationProvider
		{
			public void Configure(IContainer container, ITargetContainer targets)
			{
				throw new InvalidOperationException("The current IContainer or ITargetContainer has not been configured with a compiler configuration provider, so compilation is not possible.  The default configuration provider (used by all built-in containers) is set through the Rezolver.Compilation.CompilerConfiguration.Default property.  Most containers also allow you to provide an explicit ICompilerConfigurationProvider on construction.");
			}
		}

		private static ICompilerConfigurationProvider _defaultProvider = new NoCompilerConfigurationProvider();

		/// <summary>
		/// Gets or sets the default <see cref="ICompilerConfigurationProvider"/> used by classes derived from 
		/// <see cref="ContainerBase"/> to self-configure for target compilation.
		/// </summary>
		/// <remarks>The default implementation will throw an <see cref="InvalidOperationException"/> as soon as its
		/// <see cref="ICompilerConfigurationProvider.Configure(IContainer, ITargetContainer)"/> method is called - therefore
		/// it's imperative either that you change this provider for one which actually configures the container to support
		/// compilation, or that you explicitly pass a provider to the container when you create it.
		/// 
		/// The standard expression compiler defined in the <c>Rezolver.Compilation.Expressions</c> library defines a
		/// provider in <c>Rezolver.Compilation.Expressions.ConfigProvider</c>, and a static configuration method
		/// <c>UseAsDefaultCompiler</c> which automatically sets it into this property for you.</remarks>
		public static ICompilerConfigurationProvider DefaultProvider
		{
			get
			{
				return _defaultProvider;
			}
			set
			{
				value.MustNotBeNull(nameof(value));
				_defaultProvider = value;
			}
		}
    }
}
