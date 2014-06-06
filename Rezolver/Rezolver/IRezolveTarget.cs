using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);

		object GetObject();
		Type DeclaredType { get; }

	}
}
