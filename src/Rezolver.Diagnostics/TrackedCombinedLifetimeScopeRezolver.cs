// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Diagnostics
{
  public class TrackedCombinedLifetimeScopeRezolver : OverridingScopedContainer
  {
    private readonly int _id = TrackingUtils.NextContainerID();

    public override string ToString()
    {
      return $"(#{_id} {GetType().Name})";
    }

    protected internal ICallTracker Logger { get; private set; }

    internal TrackedCombinedLifetimeScopeRezolver(TrackedCombinedLifetimeScopeRezolver parent,
      ITargetContainer builder = null,
      ITargetCompiler compiler = null)
      : this(parent.Logger, parent, builder: builder, compiler: compiler)
    {

    }

    internal TrackedCombinedLifetimeScopeRezolver(TrackedLifetimeScopeResolver parent,
      ITargetContainer builder = null,
      ITargetCompiler compiler = null)
      : this(parent.Logger, parent, builder: builder, compiler: compiler)
    {

    }

    public TrackedCombinedLifetimeScopeRezolver(ICallTracker logger,
      IScopedContainer parentScope,
      IContainer inner = null,
      ITargetContainer builder = null,
      ITargetCompiler compiler = null)
      : base(parentScope, inner, builder ?? new TrackedRezolverBuilder(logger), compiler)
    {
      Logger = logger;
    }

    public override bool CanResolve(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.CanResolve(context), context);
    }

    public override IScopedContainer CreateLifetimeScope()
    {
      //TODO: change this to a LoggingCombinedLifetimeScopeRezolver
      return Logger.TrackCall(this, () => new TrackedCombinedLifetimeScopeRezolver(this));
    }

    public override ICompiledTarget FetchCompiled(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.FetchCompiled(context), new { context = context });
    }

    protected override object GetService(Type serviceType)
    {
      return Logger.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
    }

    public override object Resolve(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.Resolve(context), new { context = context });
    }

    public override bool TryResolve(RezolveContext context, out object result)
    {
      object tempResult = null;
      var @return = Logger.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
      result = tempResult;
      return @return;
    }

    public override void AddToScope(object obj, RezolveContext context = null)
    {
      Logger.TrackCall(this, () => base.AddToScope(obj, context), new { obj = obj, context = context });
    }

    public override IEnumerable<object> GetFromScope(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.GetFromScope(context), new { context = context });
    }

    protected override ICompiledTarget GetCompiledRezolveTarget(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
    }

    protected override ICompiledTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
    {
      return Logger.TrackCall(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
    }
  }
}
