// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
