using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Makes it possible to mix expressions and targets.
	/// 
	/// Note that this *fake* expression type does not compile - any expression tree with one of these
	/// </summary>
	public class RezolveTargetExpression : Expression
	{
		//TODO: Add override for NodeType so that we can use it in the future in the precompile stage for rezolve targets
		//to produce a proper 'compilable' expression tree
		private readonly IRezolveTarget _target;

		public RezolveTargetExpression(IRezolveTarget target)
		{
			_target = target;
		}

		public IRezolveTarget Target
		{
			get { return _target; }
		}
	}
}