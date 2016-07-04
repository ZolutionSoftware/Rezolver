// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver
{
  /// <summary>
  /// Makes it possible to mix expressions and targets.
  /// </summary>
  public class RezolveTargetExpression : Expression
  {
    //TODO: Add override for NodeType so that we can use it in the future in the precompile stage for rezolve targets
    //to produce a proper 'compilable' expression tree
    private readonly ITarget _target;

    public ITarget Target
    {
      get { return _target; }
    }

    public override Type Type
    {
      get { return _target.DeclaredType; }
    }
    public override ExpressionType NodeType
    {
      get
      {
        return ExpressionType.Extension;
      }
    }
    public override bool CanReduce
    {
      get
      {
        return true;
      }
    }
    public override Expression Reduce()
    {
      throw new NotSupportedException("RezolveTargetExpression must be rewritten as a bona-fide expression before walking the expression tree for any other purpose");
    }

    public RezolveTargetExpression(ITarget target)
    {
      _target = target;
    }
  }
}