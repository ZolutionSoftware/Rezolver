// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Logging
{
  /// <summary>
  /// Represents a series of tracked calls, potentially to different objects, that have been 
  /// tracked via an <see cref="ICallTracker"/>
  /// </summary>
  public class TrackedCallGraph
  {
    public IEnumerable<TrackedCall> Calls { get; private set; }

    public TrackedCallGraph(IEnumerable<TrackedCall> calls)
    {
      calls.MustNotBeNull(nameof(calls));
      Calls = calls;
    }
  }
}
