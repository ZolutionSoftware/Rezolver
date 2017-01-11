// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
  /// <summary>
  /// As the name suggests, the underlying target of a Rezolver call.  The output of a 
  /// target is an expression.  This allows a target that depends on another
  /// target to chain expressions together, creating specialised expression trees (and
  /// therefore specialised delegates).
  /// 
  /// The expression produced from this interface is later compiled, by an IRezolveTargetCompiler,
  /// into an ICompiledRezolveTarget - whose job it is specifically to produce object instances.
  /// </summary>
  public interface ITarget
  {
    /// <summary>
    /// If true, it is an instruction to any consumer to consider falling back to a better target
    /// configured in a more authoritative builder.  In general - almost all targets return
    /// false for this.
    /// </summary>
    bool UseFallback { get; }
    /// <summary>
    /// Gets the static type produced by this target, when executing the expression returned from a call to 
    /// <see cref="CreateExpression"/> without providing your own explicit type to be returned.
    /// </summary>
    /// <value>The type of the declared.</value>
    Type DeclaredType { get; }
    /// <summary>
    /// Returns a boolean indicating whether the target is able to produce an instance of, or an instance
    /// that is compatible with, the passed <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if <paramref name="type"/> is supported, <c>false</c> otherwise.</returns>
    /// <remarks>It is strongly suggested that you use this method to check whether the target can construct
    /// an instance of a given type rather than performing any type checking yourself on the 
    /// <see cref="DeclaredType"/>, because an <see cref="ITarget"/> might be able to support a much wider
    /// range of types other than just those which are directly compatible with its <see cref="DeclaredType"/>.</remarks>
    bool SupportsType(Type type);
  }
}
