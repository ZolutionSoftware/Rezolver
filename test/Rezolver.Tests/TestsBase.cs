using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(IRezolveTarget target, IRezolver rezolver = null)
		{
			rezolver = rezolver ?? CreateADefaultRezolver();
			var compiledTarget = new RezolveTargetDelegateCompiler().CompileTarget(target, new CompileContext(rezolver, typeof(T)));
			return (T)compiledTarget.GetObject(new RezolveContext(rezolver, typeof(T)));
		}

		protected static object GetValueFromTarget(IRezolveTarget target, IRezolver rezolver = null, Type targetType = null)
		{
			rezolver = rezolver ?? CreateADefaultRezolver();
			return new RezolveTargetDelegateCompiler().CompileTarget(target,
				new CompileContext(rezolver)).GetObject(new RezolveContext(rezolver, targetType));
		}

		protected static DefaultRezolver CreateADefaultRezolver()
		{
			return new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
		}

		protected static IRezolveTargetEntry CreateRezolverEntryForTarget(IRezolveTarget target, Type registeredType)
		{
			return new RezolveTargetEntry(registeredType, target);
		}
	}
}
