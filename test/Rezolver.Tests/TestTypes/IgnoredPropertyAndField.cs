using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  public class IgnoredPropertyAndField
  {
    private int IgnoredField;
    public int GetIgnoredField()
    {
      return IgnoredField;
    }

    private int _ignoredProperty1;
    //ignorede as it's readonly
    public int IgnoredProperty1 { get { return _ignoredProperty1; } }

    public int IgnoredProperty2 { get; private set; }

    public IgnoredPropertyAndField()
    {
      IgnoredField = 1;
      _ignoredProperty1 = 2;
      IgnoredProperty2 = 3;
    }
  }
}
