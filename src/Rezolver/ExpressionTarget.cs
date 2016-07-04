// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
  /// <summary>
  /// A generic target for all expressions not explicitly supported by a particular target.
  /// </summary>
  public class ExpressionTarget : TargetBase
  {
    private readonly Expression _expression;
    private readonly Type _declaredType;//used only when a factory is used.
    private readonly Func<CompileContext, Expression> _expressionFactory;

    public ExpressionTarget(Expression expression, ITargetAdapter adapter = null)
    {
      //TODO: null check
      _expression = expression;
    }

    public ExpressionTarget(Func<CompileContext, Expression> expressionFactory, Type declaredType, ITargetAdapter adapter = null)
    {
      _expressionFactory = expressionFactory;
      _declaredType = declaredType;
    }

    protected override Expression CreateExpressionBase(CompileContext context)
    {
      return _expression ?? _expressionFactory(context);
    }

    public override Type DeclaredType
    {
      get { return _expressionFactory != null ? _declaredType : _expression.Type; }
    }
  }
}