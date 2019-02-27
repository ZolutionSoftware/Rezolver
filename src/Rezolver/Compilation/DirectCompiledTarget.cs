// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// An <see cref="ICompiledTarget"/> which wraps around an <see cref="IDirectTarget"/>.
    ///
    /// The implementation of <see cref="GetObject(ResolveContext)"/> simply executes the target's
    /// <see cref="IDirectTarget.GetValue"/> method.
    /// </summary>
    internal class DirectCompiledTarget : ICompiledTarget
    {
        private readonly IDirectTarget _directTarget;

        public ITarget SourceTarget => this._directTarget;

        public DirectCompiledTarget(IDirectTarget target)
        {
            this._directTarget = target;
        }

        public object GetObject(ResolveContext context)
        {
            return this._directTarget.GetValue();
        }
    }
}
