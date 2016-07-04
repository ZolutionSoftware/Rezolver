using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  public class GenericHandler<T> : IHandler<T>
  {
    public string Handle(T t)
    {
      return $"This is a {typeof(T)}: {t}";
    }
  }
}
