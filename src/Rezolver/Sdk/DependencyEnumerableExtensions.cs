// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Extensions for <c>IEnumerable&lt;DependencyMetadata&gt;</c>
    /// </summary>
    public static class DependencyEnumerableExtensions
    {
        /// <summary>
        /// Identifies the objects that match the dependencies in this collection.
        /// </summary>
        /// <param name="metadata">The dependency metadata which describes a set of dependencies</param>
        /// <param name="objects">The objects from which dependencies are to be identified.</param>
        /// <returns>An enumerable containing the objects (selected from <paramref name="objects"/>) which
        /// match the dependencies in this collection.  If there are no dependency matches, the enumerable
        /// will be empty.</returns>
        public static IEnumerable<T> GetDependencies<T>(this IEnumerable<DependencyMetadata> metadata, IEnumerable<T> objects)
            where T : class
        {
            return (metadata ?? throw new ArgumentNullException(nameof(metadata)))
                .SelectMany(d => d.GetDependencies(objects))
                .Distinct(ReferenceComparer<T>.Instance);
        }
    }
}
