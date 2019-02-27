// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Compilation
{
    /// <summary>
    /// An object that produces <see cref="ICompiledTarget"/>s from <see cref="ITarget"/>s given a
    /// particular <see cref="ICompileContext"/> - for which it also acts as a factory.
    /// </summary>
    public interface ITargetCompiler
    {
        /// <summary>
        /// Create the <see cref="ICompiledTarget"/> for the given <paramref name="target"/> using the <paramref name="context"/>
        /// to inform the type of object that is to be built, and for compile-time dependency resolution.
        /// </summary>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="context">Required.  The compilation context to use for compilation.  Obtain this by calling
        /// <see cref="CreateContext(ResolveContext, ITargetContainer)"/>.</param>
        /// <returns>A compiled target which can then be used to get produce objects represented by the <paramref name="target"/>.</returns>
        ICompiledTarget CompileTarget(ITarget target, ICompileContext context);

        /// <summary>
        /// Creates a compilation context for the given <paramref name="resolveContext"/> - which is used to determine
        /// the <see cref="ResolveContext.RequestedType"/> that the eventual <see cref="ICompiledTarget"/> should return.
        /// </summary>
        /// <param name="resolveContext">The resolve context - used to get the <see cref="ResolveContext.RequestedType"/>
        /// and the <see cref="ResolveContext.Container"/>, and will be set on the <see cref="ICompileContext.ResolveContext"/>
        /// property of the returned context.</param>
        /// <param name="targets">The target container that should be used to lookup other non-compiled targets.</param>
        ICompileContext CreateContext(ResolveContext resolveContext, ITargetContainer targets);
    }
}