// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// This type is only used when using expressions as targets(via the <see cref = "ExpressionTarget" /> type) - it's
    /// functions serve no actual purpose other than to act as hooks to create specific <see cref="ITarget"/> objects
    /// in place of static code.
    ///
    /// All the functions will throw a <see cref="NotImplementedException"/> if called at runtime.
    /// </summary>
    public static class ExpressionFunctions
    {
        /// <summary>
        /// Provides a way to emit a <see cref="ResolveContext.Resolve{TResult}()"/> call to the <see cref="ResolveContext"/>
        /// which is active when an expression is compiled and executed when resolving an object.
        /// </summary>
        /// <typeparam name="T">The type to be resolved.</typeparam>
        /// <exception cref="NotImplementedException">Always.  The method is not intended to be used outside of an expression, instead
        /// it should be rewritten either to a <see cref="ResolvedTarget"/> or another <see cref="MethodCallExpression"/> bound
        /// to the <see cref="ResolveContext.Resolve{TResult}()"/> method of a <see cref="ResolveContext"/>.</exception>
        /// <remarks>Use of this function in a Lambda expression is not required if you can add a <see cref="ResolveContext"/>
        /// parameter to the Lambda - since you can simply call its <see cref="ResolveContext.Resolve{TResult}()"/> method in your
        /// lambda body.  This is primarily provided instead for non-lambda expressions which require services from the
        /// container (e.g. if manually building a <see cref="NewExpression"/> or <see cref="MethodCallExpression"/> and you
        /// want to explicitly inject one or more constructor/method arguments).</remarks>
        public static T Resolve<T>()
        {
            throw new NotImplementedException(ExceptionResources.NotRuntimeMethod);
        }

        /// <summary>
        /// Provides a way to emit a <see cref="ResolveContext.Resolve(Type)"/> call to the <see cref="ResolveContext"/>
        /// which is active when an expression is compiled and executed when resolving an object.
        /// </summary>
        /// <param name="t">The type to be resolved.</param>
        /// <exception cref="NotImplementedException">Always.  The method is not intended to be used outside of an expression, instead
        /// it should be rewritten either to a <see cref="ResolvedTarget"/> or another <see cref="MethodCallExpression"/> bound
        /// to the <see cref="ResolveContext.Resolve(Type)"/> method of a <see cref="ResolveContext"/>.</exception>
        public static object Resolve(Type _)
        {
            throw new NotImplementedException(ExceptionResources.NotRuntimeMethod);
        }
    }
}
