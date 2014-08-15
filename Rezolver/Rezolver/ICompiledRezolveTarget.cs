namespace Rezolver
{
	public interface ICompiledRezolveTarget
	{
		object GetObject();
		object GetObjectDynamic(IRezolver @dynamic);
		object GetObject(RezolveContext context);
	}
}
