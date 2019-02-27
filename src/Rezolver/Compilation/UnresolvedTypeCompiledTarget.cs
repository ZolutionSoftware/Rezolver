// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// An <see cref="ICompiledTarget"/> that can be used when a type could not be resolved.
    ///
    /// Implementations of both <see cref="GetObject"/> and <see cref="SourceTarget"/> will throw an exception
    /// if called/read.
    /// </summary>
    /// <remarks>Use of this class is encouraged when an <see cref="IContainer"/> cannot resolve a type.  Instead of
    /// checking the compiled target for a null, an instance of this can be returned in its place, but its only when the
    /// <see cref="GetObject(ResolveContext)"/> method is executed that an exception occurs.
    ///
    /// This is particularly useful when using classes such as <see cref="OverridingContainer"/>, which allow dependencies
    /// that do not exist in the base container to be fulfilled by the overriding container instead: by delaying the throwing
    /// of exceptions until the resolve operation occurs, we are able to provide that override capability.</remarks>
    public class UnresolvedTypeCompiledTarget : ICompiledTarget
    {
        private readonly Type _requestedType;

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.SourceTarget"/>
        /// </summary>
        /// <remarks>Always throws an <see cref="InvalidOperationException"/></remarks>
        public ITarget SourceTarget => throw new InvalidOperationException($"Could not resolve type {this._requestedType}");

        /// <summary>
        /// Creates a new instance of the <see cref="UnresolvedTypeCompiledTarget"/> class
        /// </summary>
        /// <param name="requestedType">Required.  The type that was requested, and which subsequently could not be resolved.</param>
        public UnresolvedTypeCompiledTarget(Type requestedType)
        {
            this._requestedType = requestedType ?? throw new ArgumentNullException(nameof(requestedType));
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(ResolveContext)"/>
        /// </summary>
        /// <param name="context">The current <see cref="ResolveContext"/></param>
        /// <returns>Always throws an <see cref="InvalidOperationException"/></returns>
        public object GetObject(ResolveContext context) => throw new InvalidOperationException($"Could not resolve type {this._requestedType}");
    }
}

namespace Rezolver
{
    using Compilation;

    /// <summary>
    /// Contains an extension to test the validity of <see cref="ICompiledTarget"/> objects.
    /// </summary>
    public static class UnresolvedICompiledTargetExtensions
    {
        /// <summary>
        /// Returns true if <paramref name="compiledTarget"/> is an <see cref="UnresolvedTypeCompiledTarget"/>
        /// (and therefore the associated could not be resolved).
        /// </summary>
        /// <param name="compiledTarget">Required.  The compiled target to be checked.</param>
        public static bool IsUnresolved(this ICompiledTarget compiledTarget)
        {
            return compiledTarget != null ? compiledTarget is UnresolvedTypeCompiledTarget
                : throw new ArgumentNullException(nameof(compiledTarget));
        }
    }
}
