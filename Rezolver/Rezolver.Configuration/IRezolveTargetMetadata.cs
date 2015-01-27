using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Represents an instruction to create an IRezolveTarget from parsed configuration.
	/// </summary>
	public interface IRezolveTargetMetadata
	{
		/// <summary>
		/// The type of rezolve target that is expected to be produced from this metadata
		/// </summary>
		RezolveTargetMetadataType Type { get; }

		/// <summary>
		/// Creates the rezolve target, optionally customised for the given target type(s), based on the given context.
		/// If the <paramref name="entry"/> is passed, then it indicates the configuration entry for which the targets are being built.
		/// </summary>
		/// <param name="targetTypes">Required.  One or more target types that the returned target is expected to be compatible with (i.e.
		/// able to build an instance of). Generally, this will be the target types for the configuration entry that is passed in 
		/// <paramref name="entry"/>.</param>
		/// <param name="context">The current context - provides access to the builder currently being constructed, as well as methods
		/// for resolving type names from <see cref="ITypeReference"/> instances or strings (and more).</param>
		/// <param name="entry">If provided, this is a reference to the configuration entry (typically an <see cref="ITypeRegistrationEntry"/>)
		/// against which this target will be registered.
		/// Please note - this doesn't mean that the target that is returned will become the target of the registration.  It might be that the
		/// target is one that is used by a parent target that will become the target of the registration.</param>
		IRezolveTarget CreateRezolveTarget(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry = null);
	}
}
