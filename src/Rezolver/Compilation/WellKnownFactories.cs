// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Contains an extension to test the validity of factories.
    /// </summary>
    internal static class WellKnownFactories
    {
        internal static class Unresolved
        {
            internal static readonly Func<ResolveContext, object> Factory = c => throw new InvalidOperationException($"Unable to resolve type {c.RequestedType}");
        }
        

        internal static class Unresolved<TService>
        {
            internal static readonly Func<ResolveContext, TService> Factory = c => throw new InvalidOperationException($"Unable to resolve type {typeof(TService)}");
        }

        /// <summary>
        /// Returns true if <paramref name="factory"/> represents a failed dependency lookup.
        /// </summary>
        /// <param name="factory">Required.  The factory to be checked.</param>
        internal static bool IsUnresolved(this Func<ResolveContext, object> factory)
        {
            return factory == Unresolved.Factory;
        }

        internal static bool IsUnresolved<TService>(this Func<ResolveContext, TService> factory)
        {
            return factory == Unresolved<TService>.Factory;
        }
    }
}
