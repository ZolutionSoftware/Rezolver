using System;
namespace Rezolver
{
	public interface ICompiledRezolveTarget
	{
		object GetObject(RezolveContext context);
	}

	//public static class ICompiledRezolveTargetExtensions
	//{
	//	public static object GetObject(this ICompiledRezolveTarget target)
	//	{
	//		return target.GetObject(RezolveContext.EmptyContext);
	//	}
	//}
}
