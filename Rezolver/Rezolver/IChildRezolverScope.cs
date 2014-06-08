namespace Rezolver.Tests
{
	public interface IChildRezolverScope : IRezolverScope
	{
		IRezolverScope ParentScope { get; }
	}
}