using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    public interface IRezolverContainer
    {
        void Register(object obj, Type type);

		  object Fetch(Type type);
	 }
}
