using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class SingletonTargetMetadata : RezolveTargetMetadataBase, Rezolver.Configuration.ISingletonTargetMetadata
	{
		/// <summary>
		/// If true, then the singleton object should be scope-compatible, i.e. with a lifetime limited
		/// to the lifetime of an external scope rather than to the AppDomain's lifetime.
		/// </summary>
		public bool Scoped { get; private set; }

		/// <summary>
		/// Metadata for the inner target that is turned into a scoped singleton
		/// </summary>
		public IRezolveTargetMetadata Inner { get; private set; }

		public SingletonTargetMetadata(IRezolveTargetMetadata inner, bool scoped = false)
			: base(RezolveTargetMetadataType.Singleton)
		{
			if (inner == null) throw new ArgumentNullException("inner");
			Inner = inner;
			Scoped = scoped;
		}

		protected override IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			return Scoped ? (IRezolveTarget)new ScopedSingletonTarget(Inner.CreateRezolveTarget(targetTypes, context, entry))
				: new SingletonTarget(Inner.CreateRezolveTarget(targetTypes, context, entry));
		}
	}
}
