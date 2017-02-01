using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
  internal class DoubleHandler : IHandler<double>
  {
    public string Handle(double t)
    {
      return $"This is a double: {t}";
    }
  }
}
