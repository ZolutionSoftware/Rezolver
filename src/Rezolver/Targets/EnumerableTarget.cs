// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Targets
{
    /// <summary>
    /// A specialised target for creating instances of <see cref="IEnumerable{T}"/>
    /// </summary>
    public class EnumerableTarget : TargetBase, IEnumerable<ITarget>
    {
        /// <summary>
        /// Always returns a concrete version of the <see cref="IEnumerable{T}"/> interface type, with
        /// <c>T</c> equal to <see cref="ElementType"/>
        /// </summary>
        public override Type DeclaredType { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Targets"/> is empty, otherwise <c>false</c>.
        /// </summary>
        public override bool UseFallback => !Targets.Any();

        /// <summary>
        /// The targets whose objects will be included in the enumerable
        /// </summary>
        public IEnumerable<ITarget> Targets { get; }

        /// <summary>
        /// The element type of the enumerable (i.e. the '<c>T</c>' in <see cref="IEnumerable{T}"/>)
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// Creates a new instance of <see cref="EnumerableTarget"/>
        /// </summary>
        /// <param name="targets">Required.  Will be set into the <see cref="Targets"/> property.  All elements must be non-null
        /// and must support the <paramref name="elementType"/> (verified by calling the <see cref="ITarget.SupportsType(Type)"/>
        /// method.</param>
        /// <param name="elementType">Required.  Will be set into the <see cref="ElementType"/> property.  Must be a concrete type - that is,
        /// it must *not* be an open generic.</param>
        public EnumerableTarget(IEnumerable<ITarget> targets, Type elementType)
        {
            Targets = targets ?? throw new ArgumentNullException(nameof(targets));
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));

            if (elementType.IsGenericType && elementType.ContainsGenericParameters)
            {
                throw new ArgumentException($"If elementType is a generic type, then it must be fully closed; {elementType} contains generic parameters", nameof(elementType));
            }

            DeclaredType = typeof(IEnumerable<>).MakeGenericType(elementType);

            if (!targets.All(t => t != null && t.SupportsType(elementType)))
            {
                throw new ArgumentException($"All targets must be non-null and support the type {elementType}", nameof(targets));
            }
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/> (through the <see cref="Targets"/> property).
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ITarget> GetEnumerator()
        {
            return Targets.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Targets.GetEnumerator();
        }
    }
}
