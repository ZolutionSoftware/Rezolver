namespace Rezolver.Tests
{
	public interface INamedRezolverScope : IRezolverScope, IChildRezolverScope
	{
		string Name { get; }
	}
}