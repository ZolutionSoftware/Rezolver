using System;

namespace Rezolver
{
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);

		object GetObject();
		Type DeclaredType { get; }

	}
}
