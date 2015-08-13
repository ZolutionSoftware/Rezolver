using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// The standard Singleton implementation of the IConfigurationAdapterContextFactory interface, and one which
	/// you can use as the starting point of your own factory.
	/// 
	/// By default, it creates a new instance of the ConfigurationAdapterContext class (using the virtual method
	/// <see cref="CreateContext(ConfigurationAdapter, IConfiguration)"/>, and then instructs it to add its default assembly references.
	/// </summary>
	public class ConfigurationAdapterContextFactory : IConfigurationAdapterContextFactory
	{
		private static readonly ConfigurationAdapterContextFactory _instance = new ConfigurationAdapterContextFactory();

		private static readonly Assembly[] _defaultReferences = new[] {
			TypeHelpers.GetAssembly(typeof(int)),
            TypeHelpers.GetAssembly(typeof(Stack<>)),
            TypeHelpers.GetAssembly(typeof(HashSet<>))
		};

		/// <summary>
		/// The one and only instance of this context factory.  Note that this is also the default application-wide context factory that is
		/// used by the standard <see cref="ConfigurationAdapter"/> class when converting configuration data into rezolvers (by virtue
		/// of the <see cref="ConfigurationAdapter.DefaultContextFactory"/> property, which you can change).
		/// </summary>
		public static ConfigurationAdapterContextFactory Instance {
			get { return _instance; }
		}

		/// <summary>
		/// Creation of new instances of this class, outside of the <see cref="Instance"/> instance, is only through inheritance.
		/// </summary>
		protected ConfigurationAdapterContextFactory() { }

		/// <summary>
		/// Gets the assemblies that are to be used for new contexts as the default set of references.
		/// 
		/// The base behaviour is to add mscorlib, System and System.Core although, depending on the target platform,
		/// the list might be less.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Assembly> GetDefaultAssemblyReferences()
		{
			return _defaultReferences;
		}

		/// <summary>
		/// Implements the <see cref="IConfigurationAdapterContextFactory"/> method of the same name.
		/// 
		/// The base behaviour is to create an instance of the <see cref="ConfigurationAdapterContext"/> class, passing
		/// the configuration and the default set of assembly references returned by <see cref="GetDefaultAssemblyReferences"/>.
		/// </summary>
		/// <param name="adapter">The adapter.</param>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		public virtual ConfigurationAdapterContext CreateContext(ConfigurationAdapter adapter, IConfiguration configuration)
		{
			return new ConfigurationAdapterContext(adapter, configuration, GetDefaultAssemblyReferences());
		}
	}
}
