using System;
using System.Collections.Generic;
using System.Text;

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
