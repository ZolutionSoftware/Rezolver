// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
  /// <summary>
  /// The default compiler for <see cref="ITarget"/> instances - takes the expression tree(s) produced by an
  /// <see cref="ITarget"/> and simply compiles it to an anonymous delegate.
  /// </summary>
  public class TargetDelegateCompiler : TargetCompilerBase
  {
    public static readonly ITargetCompiler Default = new TargetDelegateCompiler();

    public class DelegatingCompiledRezolveTarget : ICompiledTarget
    {
      private readonly Func<RezolveContext, object> _getObjectDelegate;

      public DelegatingCompiledRezolveTarget(Func<RezolveContext, object> getObjectDelegate)
      {
        _getObjectDelegate = getObjectDelegate;
      }

      public object GetObject(RezolveContext context)
      {
        return _getObjectDelegate(context);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="toCompile"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ICompiledTarget CompileTargetBase(ITarget target, Expression toCompile, CompileContext context)
    {
#if DEBUG
      var expression =
          ExpressionHelper.GetResolveLambdaForExpression(toCompile, context);
      Debug.WriteLine("Compiling Func<RezolveContext, object> from static lambda {0} for target type {1}", expression, "System.Object");
      return new DelegatingCompiledRezolveTarget(expression.Compile());
#else
			return new DelegatingCompiledRezolveTarget(
				Expression.Lambda<Func<RezolveContext, object>>(toCompile, context.RezolveContextParameter).Compile());
#endif
    }
  }
}