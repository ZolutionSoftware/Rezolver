using System.Linq.Expressions;

namespace Rezolver
{
	public interface IRezolveTargetAdapter
	{
		IRezolveTarget ConvertToTarget(Expression expression);
	}
}