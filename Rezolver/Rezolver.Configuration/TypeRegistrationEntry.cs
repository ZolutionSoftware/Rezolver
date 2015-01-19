using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class TypeRegistrationEntry : ConfigurationEntryBase, Rezolver.Configuration.ITypeRegistrationEntry
	{
		public ITypeReference[] Types { get; private set; }
		public bool IsMultipleRegistration { get; private set; }
		public IRezolveTargetMetadata TargetMetadata { get; private set; }

		public TypeRegistrationEntry(ITypeReference[] types, IRezolveTargetMetadata targetMetadata, bool isMultipleRegistration, IConfigurationLineInfo lineInfo = null)
			: base(ConfigurationEntryType.TypeRegistration, lineInfo)

		{
			if (types == null) throw new ArgumentNullException("types");
			if (types.Length == 0) throw new ArgumentException("types array must not be empty", "types");
			if (types.Any(t => t == null)) throw new ArgumentException("all types must be non-null", "types");
			if (targetMetadata == null) throw new ArgumentNullException("targetMetadata");

			Types = types;
			TargetMetadata = targetMetadata;
			IsMultipleRegistration = isMultipleRegistration;
		}
	}
}
