using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  public class DecoratorType : IDecorated
  {
    private readonly IDecorated _inner;

    public DecoratorType(IDecorated inner)
    {
      _inner = inner;
    }

    public string DoSomething()
    {
      return _inner.DoSomething() + " World";
    }
  }
}
