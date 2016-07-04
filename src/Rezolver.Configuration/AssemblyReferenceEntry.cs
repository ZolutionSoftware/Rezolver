// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class AssemblyReferenceEntry : ConfigurationEntryBase, Rezolver.Configuration.IAssemblyReferenceEntry
	{
		public string AssemblyName { get; private set; }
		public AssemblyReferenceEntry(string assemblyName, IConfigurationLineInfo lineInfo = null) : base(ConfigurationEntryType.AssemblyReference, lineInfo)
		{
			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");
			if (assemblyName.Trim().Length == 0)
				throw new ArgumentException("assemblyName cannot be all whitespace or empty", "assemblyName");
			AssemblyName = assemblyName;
		}
	}
}
