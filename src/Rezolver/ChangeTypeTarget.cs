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
  /// This target is specifically used for explicitly casting the result of one target to another type.
  /// 
  /// It's effectively the same as inserting a ConvertExpression around an expression.
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
    
    /// <summary>
    /// Always returns the target type that was passed in the <see cref="ChangeTypeTarget.ChangeTypeTarget(ITarget, Type)"/> constructor.
    /// </summary>
    public override Type DeclaredType
    {
      get
      {
        return _targetType;
      }
    }

    /// <summary>
    /// The target whose type will be changed to <see cref="DeclaredType"/>.
    /// </summary>
    public ITarget InnerTarget { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ChangeTypeTarget"/> class.
    /// </summary>
    /// <param name="innerTarget">Required.  See <see cref="InnerTarget"/></param>
    /// <param name="targetType">Required.  See <see cref="DeclaredType"/></param>
    public ChangeTypeTarget(ITarget innerTarget, Type targetType)
    {
      innerTarget.MustNotBeNull(nameof(innerTarget));
      targetType.MustNotBeNull(nameof(targetType));

      InnerTarget = innerTarget;
      _targetType = targetType;
    }
  }
}
