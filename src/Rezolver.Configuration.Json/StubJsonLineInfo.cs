// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
  internal class StubJsonLineInfo : IJsonLineInfo
  {
    internal static readonly StubJsonLineInfo Instance = new StubJsonLineInfo();
    public bool HasLineInfo()
    {
      return false;
    }

    public int LineNumber
    {
      get { throw new NotImplementedException(); }
    }

    public int LinePosition
    {
      get { throw new NotImplementedException(); }
    }
  }
}
