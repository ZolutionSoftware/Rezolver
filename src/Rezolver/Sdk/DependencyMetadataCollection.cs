// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver.Sdk
{
    /// <summary>
    /// A collection of <see cref="DependencyMetadata"/> objects.
    ///
    /// An <see cref="IDependant"/> contains one of these collections in order to store the
    /// dependencies that it has on other objects.
    /// </summary>
    public class DependencyMetadataCollection : IEnumerable<DependencyMetadata>
    {
        private readonly List<DependencyMetadata> _list
            = new List<DependencyMetadata>();

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/> - returns an enumerator
        /// which enumerates all the <see cref="DependencyMetadata"/> objects in the collection.
        /// </summary>
        public IEnumerator<DependencyMetadata> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        /// <summary>
        /// Implementation <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds a dependency to this collection.
        /// </summary>
        /// <param name="dependency">Required.  The dependency to be added.</param>
        public void Add(DependencyMetadata dependency)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            this._list.Add(dependency);
        }

        /// <summary>
        /// Adds multiple dependencies to the collection.
        /// </summary>
        /// <param name="dependencies"></param>
        public void AddRange(IEnumerable<DependencyMetadata> dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            // use a temp list to avoid enumerating twice
            List<DependencyMetadata> toAdd = new List<DependencyMetadata>();
            foreach (var dep in dependencies)
            {
                if (dep == null)
                {
                    throw new ArgumentException("All items in the enumerable must be non-null", nameof(dependencies));
                }

                toAdd.Add(dep);
            }

            this._list.AddRange(toAdd);
        }
    }
}
