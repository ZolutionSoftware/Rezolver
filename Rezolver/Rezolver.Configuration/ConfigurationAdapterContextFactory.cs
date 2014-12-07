using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// The standard implementation of the IConfigurationAdapterContextFactory interface, and one which
	/// you can use as the starting point of your own factory.
	/// 
	/// By default, it creates a new instance of the ConfigurationAdapterContext class (using the virtual method
	/// <see cref="CreateConfigurationAdapterContextInstance"/>, and then instructs it to add its default assembly references.
	/// </summary>
	public class ConfigurationAdapterContextFactory : IConfigurationAdapterContextFactory
	{
		private static readonly ConfigurationAdapterContextFactory _default = new ConfigurationAdapterContextFactory();
		/// <summary>
		/// The default context factory.  This is used automatically by the ConfigurationAdapter class if no explicit
		/// factory is passed to it on construction.
		/// </summary>
		public static ConfigurationAdapterContextFactory Default { get { return _default; } }

		/// <summary>
		/// Creation of new instances of this class, outside of the <see cref="Default"/> instance, is only through inheritance.
		/// </summary>
		protected ConfigurationAdapterContextFactory() { }

		/// <summary>
		/// Implements the <see cref="IConfigurationAdapterContextFactory"/> method of the same name.
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public virtual ConfigurationAdapterContext CreateContext(ConfigurationAdapter adapter, IConfiguration configuration)
		{
			var toReturn = CreateConfigurationAdapterContextInstance(adapter, configuration);
			toReturn.AddDefaultAssemblyReferences();
			return toReturn;
		}

		/// <summary>
		/// Called by <see cref="CreateContext"/> to create an instance of the <see cref="ConfigurationAdapterContext"/> class.
		/// 
		/// Override this if you want to customise the exact type of context that is created by the factory.
		/// </summary>
		/// <param name="adapter">The adapter for which the context is being created.</param>
		/// <param name="configuration">The configuration object that's being processed by the adapter.</param>
		/// <returns>A new instance of the <see cref="ConfigurationAdapterContext"/> class.</returns>
		protected virtual ConfigurationAdapterContext CreateConfigurationAdapterContextInstance(ConfigurationAdapter adapter, IConfiguration configuration)
		{
			return new ConfigurationAdapterContext(configuration);
		}
	}
}
