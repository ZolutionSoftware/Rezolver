using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	 public interface IRezolverScope
	 {
		  void Register(IRezolveTarget target, Type type = null, string name = null);

		  IRezolveTarget Fetch(Type type, string name = null);
	 }
}
