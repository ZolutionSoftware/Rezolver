﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver.Targets
{
    /// <summary>
    /// Wraps another target to force scoping to be ignored for the object that it produces, regardless
    /// of whether that object is IDisposable or otherwise has its own scoping behaviour.
    /// </summary>
    public class UnscopedTarget : TargetBase
    {
        /// <summary>
        /// Always returns <see cref="ScopeBehaviour.None"/>
        /// </summary>
        public override ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.None;
            }
        }

        /// <summary>
        /// Gets the declared type of object that is constructed by this target - always forwards the call
        /// to the <see cref="Inner"/> target.
        /// </summary>
        public override Type DeclaredType
        {
            get
            {
                return Inner.DeclaredType;
            }
        }

        /// <summary>
        /// Implementation of <see cref="ITarget.UseFallback" />
        /// Always forwards the call to <see cref="Inner"/> target.
        /// </summary>
        public override bool UseFallback
        {
            get
            {
                return Inner.UseFallback;
            }
        }

        /// <summary>
        /// Gets the inner target whose scoping rules are to be stripped by this target.
        /// </summary>
        public ITarget Inner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnscopedTarget"/> class.
        /// </summary>
        /// <param name="inner">Required - the inner target.</param>
        public UnscopedTarget(ITarget inner)
        {
            if(inner == null) throw new ArgumentNullException(nameof(inner));
            Inner = inner;
        }

        /// <summary>
        /// Always forward the call to the <see cref="Inner"/> target's implementation.
        /// </summary>
        /// <param name="type">The type.</param>
        public override bool SupportsType(Type type)
        {
            return Inner.SupportsType(type);
        }
    }
}
