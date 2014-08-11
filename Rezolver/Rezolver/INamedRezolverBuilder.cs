namespace Rezolver
{
	public interface INamedRezolverBuilder : IChildRezolverBuilder
	{
		string Name { get; }
	}
}