// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


namespace Rezolver
{
  /// <summary>
  /// An IRezolveTargetContainer that is a logical child of another.  Typically, a child rezolver, 
  /// if it cannot resolve a particular type, will defer to its parent for fallback.
  /// </summary>
  public interface IChildTargetContainer : ITargetContainer
  {
    ITargetContainer Parent { get; }
  }
}