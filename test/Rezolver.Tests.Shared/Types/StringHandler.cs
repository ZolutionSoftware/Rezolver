using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
  internal class StringHandler : IHandler<string>
  {
    public string Handle(string t)
    {
      return $"This is a string: {t}";
    }
  }
}
