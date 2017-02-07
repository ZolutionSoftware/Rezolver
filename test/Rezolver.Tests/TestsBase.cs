using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestsBase
	{
		protected static object GetValueFromTarget(ITarget target, Container container = null, Type targetType = null)
		{
			throw new NotImplementedException("Need to change this to use the new compilation stuff.");

			//container = container ?? CreateContainer();
			//return new TargetDelegateCompiler().CompileTarget(target,
			//  new CompileContext(container, container)).GetObject(new ResolveContext(container, targetType));
		}
	}
}
