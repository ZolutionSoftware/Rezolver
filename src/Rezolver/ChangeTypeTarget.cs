﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// This target is specifically used for explicitly typing another target.
  /// 
  /// It's effectively the same as inserting a ConvertExpression around another Expression tree
  /// 
  /// Its use is rare.
  /// </summary>
  public class ChangeTypeTarget : TargetBase
  {
    /// <summary>
    /// The target type for the conversion.
    /// </summary>
    private readonly Type _targetType;

    /// <summary>
    /// Always returns true.
    /// </summary>
    protected override bool SuppressScopeTracking
    {
      get
      {
        return true;
      }
    }
    public override Type DeclaredType
    {
      get
      {
        return _targetType;
      }
    }

    /// <summary>
    /// The target whose type will be changed to <see cref="_targetType"/>.
    /// </summary>
    public ITarget InnerTarget { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ChangeTypeTarget"/> class.
    /// </summary>
    /// <param name="innerTarget">Required.  See <see cref="InnerTarget"/></param>
    /// <param name="targetType">Required.  See <see cref="_targetType"/></param>
    public ChangeTypeTarget(ITarget innerTarget, Type targetType)
    {
      innerTarget.MustNotBeNull(nameof(innerTarget));
      targetType.MustNotBeNull(nameof(targetType));

      InnerTarget = innerTarget;
      _targetType = targetType;
    }

    protected override Expression CreateExpressionBase(CompileContext context)
    {
      var baseExpression = InnerTarget.CreateExpression(context.New(InnerTarget.DeclaredType));
      return Expression.Convert(baseExpression, _targetType);
    }
  }
}
