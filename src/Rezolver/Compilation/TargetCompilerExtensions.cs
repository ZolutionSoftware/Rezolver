// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rezolver.Compilation
{
    /// <summary>
    /// Extensions for the <see cref="ITargetCompiler"/> interface.
    /// </summary>
    public static class TargetCompilerExtensions
    {
        private static readonly MethodInfo CompileTargetGenericMethod = Extract.Method((ITargetCompiler c) => c.CompileTarget<object>(null, null)).GetGenericMethodDefinition();

        /// <summary>
        /// Performs a late-bound compilation of a strongly-typed delegate for the <paramref name="target" />.
        /// The type of the returned delegate will be <see cref="Func{T, TResult}" />, with `TResult` equal to
        /// the <see cref="ICompileContext.TargetType" /> of the <paramref name="context" />.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns>A compiled delegate</returns>
        /// <exception cref="ArgumentNullException">If any of <paramref name="compiler"/>, <paramref name="context"/> or <paramref name="target"/> are null.</exception>
        public static Delegate CompileTargetStrong(this ITargetCompiler compiler, ITarget target, ICompileContext context)
        {
            if (compiler == null) throw new ArgumentNullException(nameof(compiler));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return (Delegate)CompileTargetGenericMethod.MakeGenericMethod(
                context.TargetType)
                .Invoke(compiler, new object[] { target, context });
        }

        /// <summary>
        /// Compiles the <paramref name="target"/> by calling the
        /// <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> method of the passed
        /// <paramref name="compiler"/>, passing an <see cref="ICompileContext"/> created first by calling
        /// its <see cref="ITargetCompiler.CreateContext(ResolveContext, ITargetContainer)"/> method.
        /// </summary>
        /// <param name="compiler">Required.  The compiler which will carry out the compilation</param>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="resolveContext">Required.  The current <see cref="ResolveContext"/> - since all
        /// compilation is usually performed in response to a call to <see cref="IContainer.Resolve"/> or
        /// <see cref="IContainer.TryResolve(ResolveContext, out object)"/>.</param>
        /// <param name="targets">Required.  The <see cref="ITargetContainer"/> which contains all the registrations
        /// which might be required by the <paramref name="target"/> to obtain all its dependencies when the
        /// returned <see cref="ICompiledTarget"/> is executed.</param>
        /// <returns>A compiled target representing the passed <paramref name="target"/>, ready to be executed
        /// to obtain an object which satisfies the <see cref="ResolveContext.RequestedType"/> in the
        /// <paramref name="resolveContext"/>.</returns>
        /// <remarks>This is typically called specifically for the root object that is to be resolved from
        /// a container, hence the <see cref="ResolveContext.RequestedType"/> will match the type for which
        /// the <paramref name="target"/> is to be compiled.  Compilers must, however, always use the
        /// <see cref="ICompileContext.TargetType"/> to determine the type to be built for, because if one
        /// object requires another, then compilers do not keep creating new resolve contexts
        /// for each compilation, only new compile contexts for the same resolve context.</remarks>
        public static Func<ResolveContext, object> CompileTarget(this ITargetCompiler compiler,
            ITarget target,
            ResolveContext resolveContext,
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

            if (resolveContext == default)
            {
                throw new ArgumentNullException(nameof(resolveContext));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return compiler.CompileTarget(target, compiler.CreateContext(resolveContext, targets));
        }

        public static Func<ResolveContext, TService> CompileTarget<TService>(this ITargetCompiler compiler,
            ITarget target,
            ResolveContext resolveContext,
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

            if (resolveContext == default)
            {
                throw new ArgumentNullException(nameof(resolveContext));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return compiler.CompileTarget<TService>(target, compiler.CreateContext(resolveContext, targets));
        }
    }
}
