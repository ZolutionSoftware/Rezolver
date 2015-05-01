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

		public override IRezolveTargetEntry Fetch(Type type, string name)
		{
			var result = base.Fetch(type, name);
			//ascend the tree of rezolver builders looking for a type matching.
			//note that the name is not passed back up - that could cause untold
			//stack overflow issues!
			if (result == null && _parentBuilder != null)
				return _parentBuilder.Fetch(type, name);
			return result;
		}
	}
}