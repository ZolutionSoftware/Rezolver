using System;

namespace Rezolver
{
	public class ChildRezolverBuilder : RezolverBuilder, IChildRezolverBuilder
	{
		private readonly IRezolverBuilder _parentBuilder;

		public ChildRezolverBuilder(IRezolverBuilder parentBuilder)
		{
			parentBuilder.MustNotBeNull("parentBuilder");
			_parentBuilder = parentBuilder;

		}

		public IRezolverBuilder ParentBuilder
		{
			get { return _parentBuilder; }
		}
	}
}