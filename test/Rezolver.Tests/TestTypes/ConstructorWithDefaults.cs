using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  public class ConstructorWithDefaults : NoExplicitConstructor
  {
    public const int ExpectedValue = 1;
    public ConstructorWithDefaults(int value = ExpectedValue)
    {
      Value = value;
    }
  }
}
