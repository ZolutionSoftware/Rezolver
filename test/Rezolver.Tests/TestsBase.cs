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
      container = container ?? CreateContainer();
      var compiledTarget = new TargetDelegateCompiler().CompileTarget(target, new CompileContext(container, container, typeof(T)));
      return (T)compiledTarget.GetObject(new RezolveContext(container, typeof(T)));
    }

    protected static object GetValueFromTarget(ITarget target, Container container = null, Type targetType = null)
    {
      container = container ?? CreateContainer();
      return new TargetDelegateCompiler().CompileTarget(target,
        new CompileContext(container, container)).GetObject(new RezolveContext(container, targetType));
    }

    protected static Container CreateContainer()
    {
      return new Container(compiler: new TargetDelegateCompiler());
    }
  }
}
