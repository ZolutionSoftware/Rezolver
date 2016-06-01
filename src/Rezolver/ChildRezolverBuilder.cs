using System;

namespace Rezolver
{
	/// <summary>
	/// An implementation of IChildRezolverBuilder.
	/// 
	/// When the <see cref="Fetch(Type)"/> operation attempts to find an entry, if it
	/// cannot find one within its own registrations, it will forward the call on to
	/// its <see cref="ParentBuilder"/>.
	/// 
	/// This means that a child builder will override any registrations for a type that
	/// are present in its parent.
	/// </summary>
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

		public override IRezolveTargetEntry Fetch(Type type)
		{
			var result = base.Fetch(type);
			//ascend the tree of rezolver builders looking for a type matching.
			//note that the name is not passed back up - that could cause untold
			//stack overflow issues!
			if (result == null && _parentBuilder != null)
				return _parentBuilder.Fetch(type);
			return result;
		}
	}
}