// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// wraps an expression that represents reading or otherwise manipulating the RezolveContext that's passed
  /// into a Resolve call.
  /// </summary>
  public class RezolveContextPlaceholderExpression : Expression
  {
    private Expression _rezolveContextExpression;

    public Expression RezolveContextExpression
    {
      get
      {
        return _rezolveContextExpression;
      }
    }

    public RezolveContextPlaceholderExpression(Expression rezolveContextExpression)
    {
      _rezolveContextExpression = rezolveContextExpression;
    }

    public override bool CanReduce
    {
      get
      {
        return true;
      }
    }
    public override ExpressionType NodeType
    {
      get
      {
        return ExpressionType.Extension;
      }
    }

    public override Type Type
    {
      get
      {
        return typeof(RezolveContext);
      }
    }
  }
}
