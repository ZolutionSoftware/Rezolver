// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
    /// <summary>
    /// Contains an extension to test the validity of <see cref="ICompiledTarget"/> objects.
    /// </summary>
    public static class WellKnownFactories
    {
        internal static readonly Func<ResolveContext, object> Unresolved = c => throw new InvalidOperationException($"Unable to resolve type {c.RequestedType}");

        /// <summary>
        /// Returns true if <paramref name="factory"/> represents a failed dependency lookup.
        /// </summary>
        /// <param name="factory">Required.  The factory to be checked.</param>
        public static bool IsUnresolved(this Func<ResolveContext, object> factory)
        {
            return factory == null ? throw new ArgumentNullException(nameof(factory))
                : factory == Unresolved;
        }
    }
}
