using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static T GetValueFromTarget<T>(ITarget target, Container container = null)
		{
			throw new NotImplementedException("Need to change this to use the new compilation stuff.");

			//container = container ?? CreateContainer();
			//var compiledTarget = new TargetDelegateCompiler().CompileTarget(target, new CompileContext(container, container, typeof(T)));
			//return (T)compiledTarget.GetObject(new ResolveContext(container, typeof(T)));
		}

		protected static object GetValueFromTarget(ITarget target, Container container = null, Type targetType = null)
		{
			throw new NotImplementedException("Need to change this to use the new compilation stuff.");

			//container = container ?? CreateContainer();
			//return new TargetDelegateCompiler().CompileTarget(target,
			//  new CompileContext(container, container)).GetObject(new ResolveContext(container, targetType));
		}

		protected static Container CreateContainer()
		{
			return new Container();
		}
	}
}
