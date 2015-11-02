using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Records one or more IRezolveTargets that have been registered against a given type.
	/// 
	/// Also implements the IRezolveTarget interface so that you can request targets for the different
	/// types that are ultimately supported by the implementation based on the targets that have
	/// been added to it.
	/// </summary>
	public interface IRezolveTargetEntry : IRezolveTarget
	{
		/// <summary>
		/// The default target that is to be executed when a single instance of the registered type
		/// is requested.
		/// </summary>
		IRezolveTarget DefaultTarget { get; }
		/// <summary>
		/// A collection of targets that are to be used when an IEnumerable or other supported collection
		/// type of the registered type is requested.
		/// </summary>
		IEnumerable<IRezolveTarget> Targets { get; }
		/// <summary>
		/// Adds a target to this entry, optionally checking first to see if it already exists.   If the
		/// check is enabled (var <paramref name="checkForDuplicates"/>) then the target will not be added.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="checkForDuplicates"></param>
		/// <exception cref="System.InvalidOperationException">An implementation can throw this if it has
		/// rendered its <see cref="Targets"/> enumerable immutable after having been compiled - i.e. if 
		/// an instance has been issued from this builder, then adding more items to it would give
		/// inconsistent behaviour.</exception>
		void AddTarget(IRezolveTarget target, bool checkForDuplicates = false);
		/// <summary>
		/// If true, then the resolver should consult its fallback for an alternative instead of using this
		/// entry.
		/// </summary>
		bool UseFallback { get; }
	}
}
