// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


namespace Rezolver
{
  /// <summary>
  /// An <see cref="ITargetContainer"/> that inherits all registrations from a (<see cref="Parent"/>) target container.
  /// If it cannot resolve a target for a particular type, will defer to its parent for fallback.
  /// </summary>
  /// <seealso cref="Rezolver.ITargetContainer" />
  /// <remarks>Note that the framework does not require that enumerables of targets (retrieved by calling 
  /// <see cref="ITargetContainer.FetchAll(System.Type)"/> are *merged* between a child and parent container.
  /// 
  /// Typically, as soon as one registration exists in a child container for the same type as in the parent,
  /// it overrides all registrations in the parent for that same type.</remarks>
  public interface IChildTargetContainer : ITargetContainer
  {
    /// <summary>
    /// Gets the parent target container.
    /// </summary>
    /// <value>The parent.</value>
    ITargetContainer Parent { get; }
  }
}