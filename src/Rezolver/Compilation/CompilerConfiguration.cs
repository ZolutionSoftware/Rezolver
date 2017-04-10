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
		private static IContainerConfiguration _defaultProvider = Expressions.ExpressionCompiler.ConfigProvider;

		/// <summary>
		/// Gets or sets the default <see cref="IContainerConfiguration"/> used by classes derived from 
		/// <see cref="ContainerBase"/> to self-configure for target compilation.
		/// </summary>
		/// <remarks>The default implementation will throw an <see cref="InvalidOperationException"/> as soon as its
		/// <see cref="IContainerConfiguration.Configure(IContainer, ITargetContainer)"/> method is called - therefore
		/// it's imperative either that you change this provider for one which actually configures the container to support
		/// compilation, or that you explicitly pass a provider to the container when you create it.
		/// 
		/// The standard expression compiler defined in the <c>Rezolver.Compilation.Expressions</c> library defines a
		/// provider in <c>Rezolver.Compilation.Expressions.ConfigProvider</c>, and a static configuration method
		/// <c>UseAsDefaultCompiler</c> which automatically sets it into this property for you.</remarks>
		public static IContainerConfiguration DefaultProvider
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
