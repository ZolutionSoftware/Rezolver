using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
