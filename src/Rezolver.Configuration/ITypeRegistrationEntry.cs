using System;
namespace Rezolver.Configuration
{
	public interface ITypeRegistrationEntry : IConfigurationEntry
	{
		ITypeReference[] Types { get; }
		/// <summary>
		/// Gets a value indicating whether this instance represents a multiple registration - i.e. that
		/// when one of the <see cref="Types"/> are resolved, it's expected that an enumerable of that type
		/// will be requested, returning one or more items rather than just one.  Maps to the 
		/// <see cref="IRezolverBuilder.RegisterMultiple"/> method call.
		/// </summary>
		/// <value><c>true</c> if this instance is multiple registration; otherwise, <c>false</c>.</value>
		bool IsMultipleRegistration { get; }
		IRezolveTargetMetadata TargetMetadata { get; }
	}
}
