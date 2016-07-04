// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Diagnostics
{
  public interface ILoggingTarget
  {
    void Started(TrackedCall call);
    void Ended(TrackedCall call);
    void Result(TrackedCall call);
    void Exception(TrackedCall call);
    void Message(string message, TrackedCall call);
  }

  /// <summary>
  /// This call tracker implementation wraps around another instance, and connects it to one or more ILoggingTarget implementations.
  /// </summary>
  public class LoggingCallTracker : ICallTracker
  {
    private static readonly Lazy<LoggingCallTracker> _default = new Lazy<LoggingCallTracker>(() => new LoggingCallTracker(CallTracker.Default));

    /// <summary>
    /// A single logging call tracker that wraps around the <see cref="CallTracker.Default"/> call tracker.
    /// </summary>
    public static LoggingCallTracker Default
    {
      get
      {
        return _default.Value;
      }
    }

    private readonly ICallTracker _inner;
    private ConcurrentBag<ILoggingTarget> _targets;

    public LoggingCallTracker(ICallTracker inner, params ILoggingTarget[] targets)
    {
      inner.MustNotBeNull(nameof(inner));
      _inner = inner;
      _targets = new ConcurrentBag<ILoggingTarget>(targets);
    }

    private void InvokeOnTargets(Action<ILoggingTarget> action)
    {
      var targets = _targets.ToArray();
      foreach (var target in targets)
      {
        action(target);
      }
    }

    private void GetCallAndInvokeOnTargets(int callID, Action<TrackedCall, ILoggingTarget> action)
    {
      var call = GetCall(callID);
      if (call != null) InvokeOnTargets(t => action(call, t));
    }

    public void AddTargets(params ILoggingTarget[] targets)
    {
      targets.MustNotBeNull(nameof(targets));
      foreach (var target in targets)
      {
        _targets.Add(target);
      }
    }

    public void CallEnd(int callID)
    {
      _inner.CallEnd(callID);
      GetCallAndInvokeOnTargets(callID, (c, t) => t.Ended(c));
    }

    public void CallResult<TResult>(int callID, TResult result)
    {
      _inner.CallResult(callID, result);
      GetCallAndInvokeOnTargets(callID, (c, t) => t.Result(c));
    }

    public int CallStart(object callee, object arguments, [CallerMemberName] string method = null)
    {
      var id = _inner.CallStart(callee, arguments, method);
      GetCallAndInvokeOnTargets(id, (c, t) => t.Started(c));
      return id;
    }

    public void Exception(int callID, Exception ex)
    {
      _inner.Exception(callID, ex);
      GetCallAndInvokeOnTargets(callID, (c, t) => t.Exception(c));
    }

    public TrackedCall GetCall(int callID)
    {
      return _inner.GetCall(callID);
    }

    public TrackedCallGraph GetCompletedCalls()
    {
      return _inner.GetCompletedCalls();
    }

    public void Message(int callID, string message)
    {
      _inner.Message(callID, message);
      GetCallAndInvokeOnTargets(callID, (c, t) => t.Message(message, c));
    }
  }
}
