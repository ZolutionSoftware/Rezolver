namespace Rezolver
{
	public interface ICompiledRezolveTarget
	{
		object GetObject();
		object GetObjectDynamic(IRezolverContainer dynamicContainer);
	}
}
