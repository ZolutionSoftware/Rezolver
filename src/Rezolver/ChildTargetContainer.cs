// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
  /// <summary>
  /// A version of <see cref="TargetContainer" /> which overrides the registrations of another
  /// (the <see cref="Parent" />).
  /// </summary>
  /// <seealso cref="Rezolver.TargetContainer" />
  /// <seealso cref="Rezolver.IChildTargetContainer" />
  /// <remarks>When it's looking to find an entry for a type, if it
  /// cannot find one within its own registrations, it will forward the call on to
  /// its ancestors (starting with its <see cref="Parent" />).
  /// As a result, any dependencies required by registrations in this container can be provided by
  /// any ancestor.
  /// Fallback logic in the <see cref="Fetch(Type)" />
  /// is triggered by the <see cref="ITarget.UseFallback" /> property.</remarks>
  public class ChildTargetContainer : TargetContainer, IChildTargetContainer
  {
    private readonly ITargetContainer _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildTargetContainer"/> class.
    /// </summary>
    /// <param name="parent">Required. The parent target container</param>
    public ChildTargetContainer(ITargetContainer parent)
    {
      parent.MustNotBeNull(nameof(parent));
      _parent = parent;

    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <value>The parent.</value>
    public ITargetContainer Parent
    {
      get { return _parent; }
    }

    /// <summary>
    /// Fetches the registered target for the given <paramref name="type"/>, if found, or
    /// forwards the call to the <see cref="Parent"/> container.
    /// </summary>
    /// <param name="type">The type whose registration is sought.</param>
    /// <returns>The target which is registered for the given type, or null if no registration
    /// can be found.
    /// </returns>
    public override ITarget Fetch(Type type)
    {
      var result = base.Fetch(type);
      //ascend the tree of target containers looking for a type match.
      if ((result == null || result.UseFallback))
        return _parent.Fetch(type);
      return result;
    }


    /// <summary>
    /// Implementation of <see cref="ITargetContainer.FetchAll(Type)" />
    /// </summary>
    /// <param name="type">The type whose targets are to be retrieved.</param>
    /// <returns>A non-null enumerable containing the targets that match the type, or an
    /// empty enumerable if the type is not registered.</returns>
    public override IEnumerable<ITarget> FetchAll(Type type)
    {
      var result = base.FetchAll(type);
      if (result == null || !result.Any())
        return _parent.FetchAll(type);
      return result;
    }
  }
}