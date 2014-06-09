namespace Rezolver.Tests
{
	public interface INamedRezolverScope : IChildRezolverScope
	{
		string Name { get; }
	}
}