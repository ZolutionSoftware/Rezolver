using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Rezolver.Tests
{
	public static class ICompiledRezolveTargetExtensions
	{
		/// <summary>
		/// Invokes the compiled rezolve target with an empty rezolve context.
		/// 
		/// This might not always be suitable for calling all targets - since some might 
		/// require non-null values on one or more of the RezolveContext's properties.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static object GetObject(this ICompiledRezolveTarget target)
		{
			return target.GetObject(new RezolveContext(Mock.Of<IRezolver>(), null));
		}
	}
}
