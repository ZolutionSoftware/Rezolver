using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Provides static factory methods (including extension methods) for creating numerous types of targets 
    /// from the <see cref="Rezolver.Targets"/> namespace.
    /// </summary>
    public static partial class Target
    {
        /// <summary>
		/// Creates a <see cref="ScopedTarget"/> from the target on which this method is invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static ScopedTarget Scoped(this ITarget target)
        {
            target.MustNotBeNull(nameof(target));
            return new ScopedTarget(target);
        }

        /// <summary>
        /// Creates an <see cref="UnscopedTarget"/> from the target on which this method is invoked.
        /// </summary>
        /// <param name="target">The target.</param>
        public static UnscopedTarget Unscoped(this ITarget target)
        {
            target.MustNotBeNull(nameof(target));
            return new UnscopedTarget(target);
        }

        /// <summary>
        /// Constructs a <see cref="SingletonTarget"/> that wraps the target on which the method is invoked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static SingletonTarget Singleton(this ITarget target)
        {
            target.MustNotBeNull(nameof(target));
            return new SingletonTarget(target);
        }
    }
}
