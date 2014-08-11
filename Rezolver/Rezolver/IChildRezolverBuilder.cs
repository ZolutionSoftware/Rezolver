namespace Rezolver
{
	public interface IChildRezolverBuilder : IRezolverBuilder
	{
		IRezolverBuilder ParentBuilder { get; }
	}
}