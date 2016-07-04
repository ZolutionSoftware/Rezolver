// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reflection
{
  internal static class FieldInfoEnumerableExtensions
  {
    public static IEnumerable<FieldInfo> Public(this IEnumerable<FieldInfo> fields)
    {
      fields.MustNotBeNull(nameof(fields));
      return fields.Where(f => f.IsPublic);
    }
  }
}
