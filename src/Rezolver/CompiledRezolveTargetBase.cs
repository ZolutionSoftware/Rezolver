using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Standard starting point for an implementation of <see cref="ICompiledRezolveTarget"/> where
    /// the target is built directly from an <see cref="IRezolveTarget"/>.
    /// </summary>
    public abstract class CompiledRezolveTargetBase : ICompiledRezolveTarget
    {
        /// <summary>
        /// The target that was compiled into this instance.  Will not be null.
        /// </summary>
        protected IRezolveTarget OriginalTarget { get; }

        /// <summary>
        /// Initialises the <see cref="CompiledRezolveTargetBase"/> abstract class.
        /// </summary>
        /// <param name="originalTarget">Required - the target that was compiled into this instance.</param>
        protected CompiledRezolveTargetBase(IRezolveTarget originalTarget)
        {
            originalTarget.MustNotBeNull(nameof(originalTarget));
            OriginalTarget = originalTarget;
        }

        /// <summary>
        /// Abstract implementation of <see cref="ICompiledRezolveTarget.GetObject(RezolveContext)"/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract object GetObject(RezolveContext context);
    }
}
