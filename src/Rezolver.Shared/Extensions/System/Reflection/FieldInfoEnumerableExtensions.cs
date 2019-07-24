// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    internal static class FieldInfoEnumerableExtensions
  {
    public static IEnumerable<FieldInfo> Public(this IEnumerable<FieldInfo> fields)
    {
      if(fields == null) throw new ArgumentNullException(nameof(fields));
      return fields.Where(f => f.IsPublic);
    }
  }
}
