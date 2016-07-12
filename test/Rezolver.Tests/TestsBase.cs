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
      container = container ?? CreateADefaultRezolver();
      var compiledTarget = new TargetDelegateCompiler().CompileTarget(target, new CompileContext(container, container, typeof(T)));
      return (T)compiledTarget.GetObject(new RezolveContext(container, typeof(T)));
    }

    protected static object GetValueFromTarget(ITarget target, Container container = null, Type targetType = null)
    {
      container = container ?? CreateADefaultRezolver();
      return new TargetDelegateCompiler().CompileTarget(target,
        new CompileContext(container, container)).GetObject(new RezolveContext(container, targetType));
    }

    protected static Container CreateADefaultRezolver()
    {
      return new Container(compiler: new TargetDelegateCompiler());
    }
  }
}
