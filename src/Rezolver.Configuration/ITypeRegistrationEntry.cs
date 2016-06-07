using System;
namespace Rezolver.Configuration
{
	/// <summary>
	/// Represents an instruction to register one or more targets in an <see cref="ITargetContainer"/> against one or more 
	/// types.  Think of it as wrapping a single call to one of the builder's Register methods.
	/// </summary>
	public interface ITypeRegistrationEntry : IConfigurationEntry
	{
		/// <summary>
		/// The types for which the registration is to be made in the <see cref="ITargetContainer"/> that the entry is applied to.
		/// </summary>
		ITypeReference[] Types { get; }
		/// <summary>
		/// Gets a value indicating whether this instance represents a multiple registration - i.e. that
		/// when one of the <see cref="Types"/> are resolved, it's expected that an enumerable of that type
		/// will be requested, returning one or more items rather than just one.  Maps to the 
		/// <see cref="ITargetContainerExtensions.RegisterMultiple(ITargetContainer, System.Collections.Generic.IEnumerable{ITarget}, Type, RezolverPath)"/> method call.
		/// </summary>
		/// <value><c>true</c> if this instance is multiple registration; otherwise, <c>false</c>.</value>
		bool IsMultipleRegistration { get; }
		/// <summary>
		/// Metadata for the target(s) that is/are to be registered.
		/// </summary>
		IRezolveTargetMetadata TargetMetadata { get; }
	}
}
