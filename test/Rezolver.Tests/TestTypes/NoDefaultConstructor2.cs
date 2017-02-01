using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
  internal class NoDefaultConstructor2 : NoCtor
  {
    public const int ExpectedBestValue = 1001;
    public const string ExpectedDefaultMessage = "Default Message";
    public string Message { get; protected set; }
    public NoDefaultConstructor2(int value)
      : this(value, ExpectedDefaultMessage)
    {

    }

    public NoDefaultConstructor2(int value, string message)
    {
      if (message == null) throw new ArgumentNullException(nameof(message));
      Value = value;
      Message = message;
    }
  }
}
