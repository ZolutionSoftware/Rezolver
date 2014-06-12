namespace Rezolver
{
	public interface INamedRezolverScope : IChildRezolverScope
	{
		string Name { get; }
	}
}