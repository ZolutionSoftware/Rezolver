using System;
namespace Rezolver.Configuration
{
	/// <summary>
	/// A configuration entry instructing the configuration adapter to load an assembly before
	/// resolving types.
	/// </summary>
	public interface IAssemblyReferenceEntry : IConfigurationEntry
	{
		string AssemblyName { get; }
	}
}
