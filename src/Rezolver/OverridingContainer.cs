// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// An  <see cref="IContainer"/> which can override the resolve operations of another.  This is useful when you have a 
  /// core application-wide container, with some objects being customised based on some ambient information,
  /// e.g. configuration, MVC Area/Controller, Brand (in a multi-tenant application for example) or more.
  /// 
  /// The scoping version of this is called <see cref="OverridingScopedContainer"/>.
  /// </summary>
  public class OverridingContainer : Container
  {
    private readonly IContainer _inner;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inner">Required.  The inner rezolver that this one combines with.  Any dependencies not served
    /// by the new combined rezolver's own registry will be sought from this rezolver.</param>
    /// <param name="builder">Optional. A specific builder to be used for this rezolver's own registrations.</param>
    /// <param name="compiler">Optional. A compiler to be used to create <see cref="ICompiledTarget"/> instances
    /// from this rezolver's registrations.  If this is not provided, then the default is used (<see cref="TargetCompiler.Default"/>)</param>
    public OverridingContainer(IContainer inner, ITargetContainer builder = null, ITargetCompiler compiler = null)
      : base(builder, compiler)
    {
      inner.MustNotBeNull("inner");
      _inner = inner;
    }

    /// <summary>
    /// Called to determine if this rezolver is able to resolve the type specified in the passed <paramref name="context"/>.
    /// </summary>
    /// <param name="context">Required.  The <see cref="RezolveContext"/>.</param>
    /// <returns></returns>
    public override bool CanResolve(RezolveContext context)
    {
      return base.CanResolve(context) || _inner.CanResolve(context);
    }

    /// <summary>
    /// Overrides the base implementation to pass the lookup for an <see cref="ITarget"/> to the inner rezolver - this
    /// is how dependency chaining from this rezolver to the inner rezolver is achieved.
    /// </summary>
    /// <param name="context">Required.  The <see cref="RezolveContext"/>.</param>
    /// <returns></returns>
    protected override ICompiledTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
    {
      return _inner.FetchCompiled(context);
    }
  }
}
