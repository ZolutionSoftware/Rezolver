using System;
using Rezolver.Tests;

namespace Rezolver
{
	public class ChildRezolverScope : RezolverScope, IChildRezolverScope
	{
		private readonly IRezolverScope _parentScope;

		public ChildRezolverScope(IRezolverScope parentScope)
		{
			parentScope.MustNotBeNull("parentScope");
			_parentScope = parentScope;

		}

		public IRezolverScope ParentScope
		{
			get { return _parentScope; }
		}
	}
}