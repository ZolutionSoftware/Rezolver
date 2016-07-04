using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  public interface IRequiresInt
  {
    int IntValue { get; }
  }

  public interface IRequiresInt2
  {
    int IntValue { get; }
  }

  public class RequiresInt : IRequiresInt
  {
    public int IntValue { get; private set; }
    public RequiresInt(int intValue)
    {
      IntValue = intValue;
    }
  }
}
