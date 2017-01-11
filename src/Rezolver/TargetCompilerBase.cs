// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  public abstract class TargetCompilerBase : ITargetCompiler
  {
    public virtual ICompiledTarget CompileTarget(ITarget target, CompileContext context)
    {
      return CompileTargetBase(target, GetLambdaBody(target, context), context);
    }

    /// <summary>
    /// Produces the lambda body for the target.  The base class uses the method <see cref="ExpressionHelper.GetLambdaBodyForTarget(ITarget, CompileContext)"/>
	/// to get the expression tree that will be compiled.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual Expression GetLambdaBody(ITarget target, CompileContext context)
    {
      throw new NotImplementedException();
      //return ExpressionHelper.GetLambdaBodyForTarget(target, context);
    }

    /// <summary>
    /// Called to create an ICompiledTarget instance from the passed expression produced by the passed target for the passed context.
    /// 
    /// The expression passed into this method is constructed by a call to <see cref="GetLambdaBody(ITarget, CompileContext)"/>
    /// </summary>
    /// <param name="target">The target from which the expression <paramref name="toCompile"/> was built.  Note - this expression will
    /// have been optimised and potentially rewritten ready for compilation, and will likely not be equal to the expression originally 
    /// produced by its own <see cref="ITarget.CreateExpression(CompileContext)"/> method.</param>
    /// <param name="toCompile">The expression built from <paramref name="target"/> by this instance's own <see cref="GetLambdaBody(ITarget, CompileContext)"/>
    /// methodd.</param>
    /// <param name="context">The context for which the compilation is being performed.</param>
    /// <returns></returns>
    protected abstract ICompiledTarget CompileTargetBase(ITarget target, Expression toCompile, CompileContext context);
  }
}
