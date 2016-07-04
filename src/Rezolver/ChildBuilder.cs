// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
  /// <summary>
  ///	A Builder which acts as a child of another builder.
  /// 
  /// When it's looking to find an entry for a type, if it
  /// cannot find one within its own registrations, it will forward the call on to
  /// its <see cref="Parent"/>.
  /// 
  /// This means that a child builder can override any registrations that
  /// are present in its parent.
  /// </summary>
  public class ChildBuilder : Builder, IChildTargetContainer
  {
    private readonly ITargetContainer _parent;

    public ChildBuilder(ITargetContainer parent)
    {
      parent.MustNotBeNull(nameof(parent));
      _parent = parent;

    }

    public ITargetContainer Parent
    {
      get { return _parent; }
    }

    public override ITarget Fetch(Type type)
    {
      var result = base.Fetch(type);
      //ascend the tree of rezolver builders looking for a type matching.
      //note that the name is not passed back up - that could cause untold
      //stack overflow issues!
      if (result == null && _parent != null)
        return _parent.Fetch(type);
      return result;
    }
    public override IEnumerable<ITarget> FetchAll(Type type)
    {
      var result = base.FetchAll(type);
      if (result == null || !result.Any())
        return _parent.FetchAll(type);
      return result;
    }
  }
}