// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Runtime
{
    internal class TargetProjection
    {
        /// <summary>
        /// The target that will implement the projection
        /// </summary>
        public ITarget Target { get; }
        /// <summary>
        /// Concrete type to be built by the <see cref="Target"/> for the projection
        /// </summary>
        public Type ImplementationType { get; }

        public TargetProjection(ITarget target, Type implementationType)
        {
            Target = target;
            ImplementationType = implementationType;
        }
    }
}
