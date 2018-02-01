// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Represents a binding to a method whose arguments will be supplied by <see cref="ITarget" /> instances.
    /// </summary>
    public class MethodBinding
    {
        /// <summary>
        /// Gets the method to be invoked.
        /// </summary>
        /// <value>The method.</value>
        public MethodBase Method { get; }
        /// <summary>
        /// Gets the argument bindings for the method call.
        ///
        /// Never null but can be empty.
        /// </summary>
        /// <value>The bound arguments.</value>
        public ParameterBinding[] BoundArguments { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodBinding"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="boundArgs">Optional.  The bound arguments.  Can be null or empty.</param>
        public MethodBinding(MethodBase method, ParameterBinding[] boundArgs = null)
        {
            method.MustNotBeNull(nameof(method))
                .MustNot(m => m.IsAbstract, "Method cannot be abstract", nameof(method));

            if (boundArgs != null)
            {
                var parameters = method.GetParameters();
                boundArgs.MustNot(pbs => pbs.Any(pb => pb == null), "All parameter bindings must be non-null", nameof(boundArgs))
                    .Must(pbs => pbs.All(pb => parameters.Contains(pb.Parameter)), "All parameter bindings must be for parameters declared on the method", nameof(boundArgs));
            }

            this.Method = method;
            this.BoundArguments = boundArgs ?? ParameterBinding.None;
        }
    }
}
