// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


namespace Rezolver
{
  /// <summary>
  /// An <see cref="ITargetContainer"/> that inherits all registrations from an ancestor (<see cref="Parent"/>).
  /// If it cannot resolve a target for a particular type, will defer to its parent for fallback.
  /// </summary>
  /// <seealso cref="Rezolver.ITargetContainer" />
  public interface IChildTargetContainer : ITargetContainer
  {
    /// <summary>
    /// Gets the parent target container.
    /// </summary>
    /// <value>The parent.</value>
    ITargetContainer Parent { get; }
  }
}