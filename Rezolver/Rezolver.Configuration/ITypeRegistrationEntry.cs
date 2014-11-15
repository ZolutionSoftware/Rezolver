using System;
namespace Rezolver.Configuration
{
	public interface ITypeRegistrationEntry : IConfigurationEntry
	{
		ITypeReference[] Types { get; }

		IRezolveTargetMetadata TargetMetadata { get; }
	}
}
