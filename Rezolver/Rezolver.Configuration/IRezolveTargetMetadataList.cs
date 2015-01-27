using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Represents a list of IRezolveTargetMetadata instances - for when you want to register multiple targets against a single type.
	/// </summary>
	public interface IRezolveTargetMetadataList : IRezolveTargetMetadata
	{
		/// <summary>
		/// Gets the list of targets that will be used to construct the array.
		/// 
		/// Note - a list is used to allow for modification of the targets after initial creation.
		/// </summary>
		/// <value>The targets.</value>
		IList<IRezolveTargetMetadata> Targets { get; }

		/// <summary>
		/// Replacement for <see cref="IRezolveTargetMetadata"/> for this interface.
		/// Creates the rezolve target, potentially customised for the given target type(s), based on the given context.
		/// If the <paramref name="entry"/> is passed, then it indicates the configuration entry for which the targets are being built.
		/// </summary>
		/// <param name="targetTypes">The target types for each of the returned target, generally, this will be the ultimate target types
		/// for the configuration entry that is passed in <paramref name="entry"/>. An implementation is not bound to use these types 
		/// at all, but it helps provide additional context that might be of use when generating the rezolve target.</param>
		/// <param name="context">The context.</param>
		/// <param name="entry">If provided, this is a reference to the configuration entry for which this target is being built.</param>
		IEnumerable<IRezolveTarget> CreateRezolveTargets(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry);
	}
}
