// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// Describes a type which discovers property/field bindings
  /// </summary>
  public interface IPropertyBindingBehaviour
  {
    /// <summary>
    /// Retrieves the property and/or field bindings for the given type based on the given <see cref="CompileContext"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    PropertyOrFieldBinding[] GetPropertyBindings(CompileContext context, Type type);
  }

}
