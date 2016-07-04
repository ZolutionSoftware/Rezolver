// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
  public static partial class ITargetContainerExtensions
  {
    /// <summary>
    /// Registers a single instance (already created) to be used when resolving a particular service type.
    /// 
    /// If using a scope, then the object will be tracked in the rootmost scope so that, if it's disposable,
    /// it will be disposed when the root scope is disposed.
    /// </summary>
    /// <typeparam name="T">Type of the object - will be used as the service type for registration if
    /// <paramref name="serviceType"/> is not provied.</typeparam>
    /// <param name="builder"></param>
    /// <param name="obj">The object to be returned when resolving.</param>
    /// <param name="serviceType">The service type against which this object is to be registered, if different
    /// from <typeparamref name="T"/>.</param>
    /// <param name="suppressScopeTracking"></param>
    public static void RegisterObject<T>(this ITargetContainer builder, T obj, Type serviceType = null, bool suppressScopeTracking = true)
    {
      builder.MustNotBeNull(nameof(builder));
      builder.Register(obj.AsObjectTarget(serviceType, suppressScopeTracking: suppressScopeTracking), serviceType);
    }
  }
}
