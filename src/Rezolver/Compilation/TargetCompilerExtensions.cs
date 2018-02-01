// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// Extensions for the <see cref="ITargetCompiler"/> interface.
    /// </summary>
    public static class TargetCompilerExtensions
    {
        /// <summary>
        /// Compiles the <paramref name="target"/> by calling the
        /// <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> method of the passed
        /// <paramref name="compiler"/>, passing an <see cref="ICompileContext"/> created first by calling
        /// its <see cref="ITargetCompiler.CreateContext(IResolveContext, ITargetContainer)"/> method.
        /// </summary>
        /// <param name="compiler">Required.  The compiler which will carry out the compilation</param>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="resolveContext">Required.  The current <see cref="IResolveContext"/> - since all
        /// compilation is usually performed in response to a call to <see cref="IContainer.Resolve"/> or
        /// <see cref="IContainer.TryResolve(IResolveContext, out object)"/>.</param>
        /// <param name="targets">Required.  The <see cref="ITargetContainer"/> which contains all the registrations
        /// which might be required by the <paramref name="target"/> to obtain all its dependencies when the
        /// returned <see cref="ICompiledTarget"/> is executed.</param>
        /// <returns>A compiled target representing the passed <paramref name="target"/>, ready to be executed
        /// to obtain an object which satisfies the <see cref="IResolveContext.RequestedType"/> in the
        /// <paramref name="resolveContext"/>.</returns>
        /// <remarks>This is typically called specifically for the root object that is to be resolved from
        /// a container, hence the <see cref="IResolveContext.RequestedType"/> will match the type for which
        /// the <paramref name="target"/> is to be compiled.  Compilers must, however, always use the
        /// <see cref="ICompileContext.TargetType"/> to determine the type to be built for, because if one
        /// object requires another, then compilers do not keep creating new resolve contexts
        /// for each compilation, only new compile contexts for the same resolve context.</remarks>
        public static ICompiledTarget CompileTarget(this ITargetCompiler compiler,
            ITarget target,
            IResolveContext resolveContext,
            ITargetContainer targets)
        {
            if (compiler == null)
            {
                throw new ArgumentNullException(nameof(compiler));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (resolveContext == null)
            {
                throw new ArgumentNullException(nameof(resolveContext));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return compiler.CompileTarget(target, compiler.CreateContext(resolveContext, targets));
        }
    }
}
