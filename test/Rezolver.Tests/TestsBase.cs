using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(ITarget target, IContainer rezolver = null)
		{
			rezolver = rezolver ?? CreateADefaultRezolver();
			var compiledTarget = new TargetDelegateCompiler().CompileTarget(target, new CompileContext(rezolver, typeof(T)));
			return (T)compiledTarget.GetObject(new RezolveContext(rezolver, typeof(T)));
		}

		protected static object GetValueFromTarget(ITarget target, IContainer rezolver = null, Type targetType = null)
		{
			rezolver = rezolver ?? CreateADefaultRezolver();
			return new TargetDelegateCompiler().CompileTarget(target,
				new CompileContext(rezolver)).GetObject(new RezolveContext(rezolver, targetType));
		}

		protected static Container CreateADefaultRezolver()
		{
			return new Container(compiler: new TargetDelegateCompiler());
		}
	}
}
