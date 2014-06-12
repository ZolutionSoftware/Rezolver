namespace Rezolver
{
	public interface IChildRezolverScope : IRezolverScope
	{
		IRezolverScope ParentScope { get; }
	}
}