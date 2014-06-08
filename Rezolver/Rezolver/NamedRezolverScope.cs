using System;

namespace Rezolver.Tests
{
	public class NamedRezolverScope : RezolverScope, INamedRezolverScope
	{
		private readonly IRezolverScope _parentScope;
		private readonly string _name;

		public NamedRezolverScope(IRezolverScope parentScope, string name)
		{
			_parentScope = parentScope;
			_name = name;
		}

		public IRezolverScope ParentScope
		{
			get { return _parentScope; }
		}

		public string Name
		{
			get { return _name; }
		}
	}
}