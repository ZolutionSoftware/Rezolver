// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Compilation
{
  /// <summary>
  /// An object that produces <see cref="ICompiledTarget"/>s from <see cref="ITarget"/>s given a 
  /// particular <see cref="ICompileContext"/>.
  /// </summary>
  public interface ITargetCompiler
  {
    /// <summary>
    /// Create the <see cref="ICompiledTarget"/> for the given <paramref name="target"/> using the <paramref name="context"/>
    /// to inform the type of object that is to be built, and for compile-time dependency resolution.
    /// </summary>
    /// <param name="target">Required.  The target to be compiled.</param>
    /// <param name="context">Required.  The current compilation context.</param>
    /// <returns>A compiled target which can then be used to get produce objects represented by the <paramref name="target"/>.</returns>
    ICompiledTarget CompileTarget(ITarget target, ICompileContext context);
  }
}