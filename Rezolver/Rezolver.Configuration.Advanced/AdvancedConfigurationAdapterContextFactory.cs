using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Configuration
{
	/// <summary>
	/// An advanced factory which automatically loads and references all assemblies that are deployed in the 
	/// application's base directory and any subfolders determined by the <see cref="System.AppDomain"/>'s SetupInformation's
	/// PrivateBinBath.
	/// 
	/// This is a singleton - accessed through the <see cref="Instance"/> property.
	/// </summary>
	/// <remarks>The <see cref="ConfigurationAdapterContextFactory"/> has a very limited set of default assembly references,
	/// because it's a portable class, the AppDomain introspection APIs are not available.  As a result, to simplify type
	/// references in a configuration file loaded in a portable environment, you need to add assembly references manually.
	/// 
	/// This class, on the other hand, is targeted at environments where these APIs are available, and should effectively remove
	/// the need to add any references at all, meaning a configuration file can use shorter type names.
	/// </remarks>
	public partial class AdvancedConfigurationAdapterContextFactory : ConfigurationAdapterContextFactory
	{
#if DEBUG && TRACE
		static void Log(string format, params object[] formatArgs)
		{
			Trace.WriteLine(string.Concat("[AdvancedConfigurationAdapterContextFactory]: ", string.Format(format, formatArgs)));
		}
#else
		static partial void Log(string format, params object[] formatArgs);
#endif

		private static readonly IConfigurationAdapterContextFactory _instance = new AdvancedConfigurationAdapterContextFactory();
		
		/// <summary>
		/// Gets the one and only instance of the <see cref="AdvancedConfigurationAdapterContextFactory"/>.
		/// </summary>
		/// <value>The instance.</value>
		public static new IConfigurationAdapterContextFactory Instance { get { return _instance; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="AdvancedConfigurationAdapterContextFactory"/> class.
		/// 
		/// Class is creatable only through inheritance.  
		/// </summary>
		protected AdvancedConfigurationAdapterContextFactory()
		{

		}

		private static IEnumerable<string> GetBinFolders()
		{
			List<string> toReturn = new List<string>();
			Log("Probing {0}", AppDomain.CurrentDomain.BaseDirectory);
			toReturn.Add(AppDomain.CurrentDomain.BaseDirectory);
			if(!string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath))
			{
				foreach(var path in AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
				{
					if(!Path.IsPathRooted(path))
					{
						Log("Probing {0}", path);
						toReturn.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
					}
				}
			}

			return toReturn;
		}

		private static IEnumerable<AssemblyName> GetDeployedAssemblyNames()
		{
			foreach (var path in GetBinFolders())
			{
				foreach(var assemblyName in GetDeployedAssemblyNamesFromPath(path))
				{
					yield return assemblyName;
				}
			}
		}

		private static IEnumerable<AssemblyName> GetDeployedAssemblyNamesFromPath(string p)
		{
			//S.O. NOTE: ELIDED - ALL EXCEPTION HANDLING FOR BREVITY

			//get all .dll files from the specified path and load the lot
			FileInfo[] files = null;
			//you might not want recursion - handy for localised assemblies 
			//though especially.
			files = new DirectoryInfo(p).GetFiles("*.dll",
					SearchOption.TopDirectoryOnly);

			return files.Select(fi => AssemblyName.GetAssemblyName(fi.FullName));
		}

		//probably going to need http://stackoverflow.com/questions/3021613/how-to-pre-load-all-deployed-assemblies-for-an-appdomain
		private static IEnumerable<Assembly> GetAllAssemblyReferences(Assembly assembly, Dictionary<string, Assembly> inProcess)
		{
			if (inProcess == null)
				inProcess = new Dictionary<string, Assembly>();
			List<Assembly> temp = null;
			foreach(var referenceName in assembly.GetReferencedAssemblies())
			{
				if(!inProcess.ContainsKey(referenceName.FullName))
				{
					try
					{
						var reference = Assembly.Load(referenceName);
						inProcess.Add(referenceName.FullName, reference);
						temp.Add(reference);
						temp.AddRange(GetAllAssemblyReferences(reference, inProcess));
					}
					catch (Exception) { /* don't do anything with this exception - it's not relevant */ }
				}
			}
			return temp;
		}

		/// <summary>
		/// Gets the assemblies that are to be used for new contexts as the default set of references.
		/// 
		/// This override gets all the assemblies that are deployed with the application.  Note that this
		/// has the effect of pre-loading all those assemblies.
		/// </summary>
		/// <returns>IEnumerable&lt;System.Reflection.Assembly&gt;.</returns>
		protected override IEnumerable<System.Reflection.Assembly> GetDefaultAssemblyReferences()
		{
			List<Assembly> toReturn = new List<Assembly>();
			Assembly toAdd = null;
			foreach(var assemblyName in GetDeployedAssemblyNames())
			{
				toAdd = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
					assemblyName.FullName == assembly.GetName().FullName);
				if (toAdd == null)
				{
					//crucial - USE THE ASSEMBLY NAME.
					//in a web app, this assembly will automatically be bound from the 
					//Asp.Net Temporary folder from where the site actually runs.
					Log("Loading {0}", assemblyName.FullName);
					
				}
				else
				{
					Log("{0} Already loaded", assemblyName.FullName);
				}
				toReturn.Add(Assembly.Load(assemblyName));
			}
			toReturn.AddRange(base.GetDefaultAssemblyReferences());
			return toReturn;
		}
	}
}
