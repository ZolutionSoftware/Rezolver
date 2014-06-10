using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Tests;

namespace Rezolver
{
	public interface IRezolverScope
	{
		void Register(IRezolveTarget target, Type type = null, string name = null);

		IRezolveTarget Fetch(Type type, string name = null);
		INamedRezolverScope GetNamedScope(string name, bool create = false);
	}
}
