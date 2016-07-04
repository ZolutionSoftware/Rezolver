// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
namespace Rezolver.Configuration
{
  /// <summary>
  /// Configuration metadata for building a <see cref="ListTarget"/> in a RezolverBuilder.
  /// </summary>
  public interface IListTargetMetadata : IRezolveTargetMetadata
  {
    /// <summary>
    /// Gets the declared element type of the array or list that will be created from this metadata.
    /// </summary>
    /// <value>The type of the element.</value>
    ITypeReference ElementType { get; }
    /// <summary>
    /// Gets the metadata for the targets that will be used for the items that'll be returned
    /// in the Array or List that will be created by the <see cref="ListTarget"/> created from this metadata.
    /// </summary>
    /// <value>The elements.</value>
    IRezolveTargetMetadataList Items { get; }

    /// <summary>
    /// Maps to the <see cref="ListTarget.AsArray"/> property.  If true, then an array of <see cref="ElementType"/>
    /// will be created, otherwise a List&lt;<see cref="ElementType"/>&gt; will be created by the ListTarget
    /// created from this metadata.
    /// </summary>
    /// <value><c>true</c> if this instance represents a ListTarget that will create an array; otherwise, <c>false</c>.</value>
    bool IsArray { get; }
  }
}
