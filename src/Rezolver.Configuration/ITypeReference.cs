// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
  /// <summary>
  /// Captures a reference to a type made in a configuration file.  It does not guarantee that the type
  /// can be located, it simply provides a common interface for storing the type information written
  /// in a configuration file.
  /// 
  /// An IConfigurationAdapter instance will need to resolve the actual runtime type from this when registering
  /// targets from a configuration file.
  /// </summary>
  public interface ITypeReference : IConfigurationLineInfo
  {
    /// <summary>
    /// The root type name.
    /// </summary>
    string TypeName { get; }
    /// <summary>
    /// Any explicitly provided generic arguments are stored here.
    /// 
    /// Note that it might turn out that the TypeName refers to a whole closed generic type, in which
    /// case the referenced type could still be generic even if this array is empty.
    /// 
    /// It's also the case that arguments could be passed here when the root type name resolves to
    /// a non-generic type definition, in which case type resolution will likely fail.
    /// </summary>
    ITypeReference[] GenericArguments { get; }
    /// <summary>
    /// True if this type represents an open generic argument - this is how to explicitly reference an open generic type in a type reference:
    /// you specify a base type, then have one or more open generic arguments specified in the GenericArguments array.  If all are
    /// open generic arguments, then you have created a reference to the fully open generic type.
    /// 
    /// You only need to provide all-open arguments if the base <see cref="TypeName"/> could be ambiguous between a non generic and generic type,
    /// or there are multiple generic types with the same base name.
    /// 
    /// Equally, you can do this to create references to partially open generics, which may or may not be supported by the adapter or
    /// the target that is built.
    /// </summary>
    bool IsOpenGenericTypeArgument { get; }

    /// <summary>
    /// Gets a value indicating whether this instance represents a type that is to be late-bound for a specific target type.
    /// </summary>
    /// <value><c>true</c> if this instance is unbound; otherwise, <c>false</c>.</value>
    bool IsUnbound { get; }
    /// <summary>
    /// Gets a value indicating whether the type ultimately represented by this instance is an array of the
    /// type described by the rest of this instance's properties.
    /// </summary>
    /// <value><c>true</c> if this instance represents an array type; otherwise, <c>false</c>.</value>
    bool IsArray { get; }
  }
}
