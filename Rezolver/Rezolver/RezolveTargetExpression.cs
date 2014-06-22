using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Makes it possible to mix expressions and targets.
	/// 
	/// Note that this *fake* expression typee does not compile.
	/// </summary>
	public class RezolveTargetExpression : Expression
	{
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