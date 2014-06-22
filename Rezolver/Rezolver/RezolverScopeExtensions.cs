using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rezolver.Resources;

namespace Rezolver
{
	public static class RezolverScopeExtensions
	{
		public static T Rezolve<T>(this IRezolverScope scope)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}

		public static T Rezolve<T>(this IRezolverScope scope, string name)
		{
			throw new NotImplementedException(Exceptions.NotRuntimeMethod);
		}
	}
}
