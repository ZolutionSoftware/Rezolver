using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	 public interface IRezolverContainer
	 {
		  void Register(IRezolveTarget target, Type type = null);

		  IRezolveTarget Fetch(Type type);
	 }
}
