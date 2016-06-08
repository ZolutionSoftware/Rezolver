﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Standard starting point for an implementation of <see cref="ICompiledTarget"/> where
    /// the target is built directly from an <see cref="ITarget"/>.
    /// </summary>
    public abstract class CompiledTargetBase : ICompiledTarget
    {
        /// <summary>
        /// The target that was compiled into this instance.  Will not be null.
        /// </summary>
        protected ITarget OriginalTarget { get; }

        /// <summary>
        /// Initialises the <see cref="CompiledTargetBase"/> abstract class.
        /// </summary>
        /// <param name="originalTarget">Required - the target that was compiled into this instance.</param>
        protected CompiledTargetBase(ITarget originalTarget)
        {
            originalTarget.MustNotBeNull(nameof(originalTarget));
            OriginalTarget = originalTarget;
        }

        /// <summary>
        /// Abstract implementation of <see cref="ICompiledTarget.GetObject(RezolveContext)"/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract object GetObject(RezolveContext context);
    }
}