using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
  internal interface IHandler<T>
  {
    string Handle(T t);
  }
}
