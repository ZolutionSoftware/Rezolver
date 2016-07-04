// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// Wraps a rezolve context for expression building - used principally in the ConstruuctorTarget class,
  /// but others are free to use it also.
  /// </summary>
  public class RezolveContextExpressionHelper : RezolveContext
  {
    /// <summary>
    /// Private constructor to prevent an instance of this type from ever being constructed
    /// </summary>
    private RezolveContextExpressionHelper() : base(null, null) { }

    public T Resolve<T>()
    {
      throw new NotImplementedException(ExceptionResources.NotRuntimeMethod);
    }
  }
}
