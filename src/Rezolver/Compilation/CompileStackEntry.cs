// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Compilation
{
    /// <summary>
    /// Represents an entry in the compilation stack of a <see cref="ICompileContext"/>,
    /// recording both a target that is being compiled, and the type for which it is being compiled.
    /// </summary>
    public struct CompileStackEntry : IEquatable<CompileStackEntry>
    {
        /// <summary>
        /// Gets the target being compiled.
        /// </summary>
        public ITarget Target { get; }
        /// <summary>
        /// Gets the type for which the target is being compiled.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileStackEntry"/> class.
        /// </summary>
        /// <param name="target">The target being compiled.</param>
        /// <param name="targetType">Type for which the target is being compiled.</param>
        public CompileStackEntry(ITarget target, Type targetType)
        {
            if(target == null) throw new ArgumentNullException(nameof(target));
            if(targetType == null) throw new ArgumentNullException(nameof(targetType));

            Target = target;
            TargetType = targetType;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <remarks>Equality is defined as both the <see cref="Target"/> and the <see cref="TargetType"/>
        /// being equal between this instance and the <paramref name="other"/> instance.</remarks>
        public bool Equals(CompileStackEntry other)
        {
            return object.ReferenceEquals(Target, other.Target) && TargetType.Equals(other.TargetType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj)
        {
            if (obj is CompileStackEntry se)
            {
                return Equals(se);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode() ^ TargetType.GetHashCode();
        }
    }
}
