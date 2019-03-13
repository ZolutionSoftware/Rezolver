// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// A reusable implementation of <see cref="ICompiledTarget"/> for when the compiled code
    /// for an <see cref="ITarget"/> can be represented by a delegate.
    /// </summary>
    public class DelegatingCompiledTarget : ICompiledTarget
    {
        private readonly Func<ResolveContext, object> _callback;

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.SourceTarget"/>
        /// </summary>
        public ITarget SourceTarget { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DelegatingCompiledTarget"/> class.
        /// </summary>
        /// <param name="callback">Required.  The delegate to be executed when
        /// <see cref="GetObject(ResolveContext)"/> is called.</param>
        /// <param name="sourceTarget">Required.  The <see cref="ITarget"/> from which this
        /// <see cref="DelegatingCompiledTarget"/> is constructed.</param>
        public DelegatingCompiledTarget(Func<ResolveContext, object> callback, ITarget sourceTarget)
        {
            this._callback = callback ?? throw new ArgumentNullException(nameof(callback));
            SourceTarget = sourceTarget ?? throw new ArgumentNullException(nameof(sourceTarget));
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(ResolveContext)" /> - simply
        /// executes the delegate passed on construction.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object GetObject(ResolveContext context)
        {
            return this._callback(context);
        }
    }
}
