// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Abstract base class for implementations of <see cref="IExpressionBuilder{TTarget}"/>.
    ///
    /// Provide an implementation of <see cref="Build(TTarget, IExpressionCompileContext, IExpressionCompiler)"/> and then register
    /// an instance in an <see cref="ObjectTarget"/> in the active container.
    /// </summary>
    /// <typeparam name="TTarget">The type of target for which this builder can build an expression.  This should ideally be
    /// a type which directly implements <see cref="ITarget"/> - however, so long as the runtime type of all the targets which
    /// are fed to it do implement it, everything is fine.
    /// </typeparam>
    /// <seealso cref="Rezolver.Compilation.Expressions.IExpressionBuilder{TTarget}" />
    /// <seealso cref="ExpressionBuilderBase"/>
    /// <remarks>This is a generic extension of the <see cref="ExpressionBuilderBase"/> class,
    /// designed to simplify the process of implementating the <see cref="IExpressionBuilder{TTarget}"/>
    /// (and, by extension, <see cref="IExpressionBuilder"/>) interface.
    ///
    /// This is the class from which most of the built-in expression builders derive, because the
    /// <see cref="ExpressionCompiler"/>, when asked to compile an expression, attempts to resolve
    /// an <see cref="IExpressionBuilder{TTarget}"/> whose <typeparamref name="TTarget"/>
    /// is the same type as the target that needs compiling.
    ///
    /// Inheriting from <see cref="ExpressionBuilderBase"/> is more appropriate if your builder is capable
    /// of handling multiple types of <see cref="ITarget"/> - a scenario that's much less common.
    ///
    /// Note that this class' implementation of <see cref="IExpressionBuilder{TTarget}"/> is entirely explicit and
    /// non-virtual, the same as with its base class, hence the only way to build an expression via an instance
    /// of this class without exposing the behaviour to external callers yourself is via the interface.
    /// </remarks>
    public abstract class ExpressionBuilderBase<TTarget> : ExpressionBuilderBase, IExpressionBuilder<TTarget>
    {
        /// <summary>
        /// Overrides the abstract <see cref="ExpressionBuilderBase.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> (and seals it from
        /// further overrides); checks that <paramref name="target" /> is an instance of <typeparamref name="TTarget" />
        /// (throwing an <see cref="ArgumentException" /> if not) and then calls this class' <see cref="Build(TTarget, IExpressionCompileContext, IExpressionCompiler)" />
        /// abstract function.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built.  Must be an instance of <typeparamref name="TTarget" />.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        /// <exception cref="System.ArgumentException">target must be an instance of { typeof(TTarget) }</exception>
        /// <exception cref="ArgumentException">If the passed target is not an instance of <typeparamref name="TTarget" /></exception>
        protected sealed override Expression Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            TTarget target2;
            try
            {
                target2 = (TTarget)target;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException($"target must be an instance of {typeof(TTarget)}", nameof(target));
            }

            return Build(target2, context, compiler);
        }

        /// <summary>
        /// Builds the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <param name="compiler">Optional. The compiler that's requesting the expression; and which can be used
        /// to compile other targets within the target.  If not provided, then the implementation attempts to locate
        /// the context compiler using the <see cref="ExpressionBuilderBase.GetContextCompiler(IExpressionCompileContext)"/> method, and will throw
        /// an <see cref="InvalidOperationException"/> if it cannot do so.</param>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null or <paramref name="context"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="compiler"/> is null and an IExpressionCompiler
        /// couldn't be resolved for the current context (via <see cref="ExpressionBuilderBase.GetContextCompiler(IExpressionCompileContext)"/></exception>
        Expression IExpressionBuilder<TTarget>.Build(TTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // note that this is a copy of the code found in the non-generic ExpressionBuilderBase's explicit implementation
            // of IExpressionBuilder.Build because there's no simple way to share the implementation between two explicit
            // implementations of interface methods.

            // can't use the MustNotBeNull extn method because no class constraint on TTarget
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if(context == null) throw new ArgumentNullException(nameof(context));

            if (compiler == null)
            {
                compiler = GetContextCompiler(context);
                if (compiler == null)
                {
                    throw new InvalidOperationException("Unable to identify the IExpressionCompiler for the current context");
                }
            }

            if (target is ITarget tTarget)
            {
                return BuildCore(tTarget, context, compiler);
            }
            else
            {
                throw new ArgumentException($"The target must implement ITarget as well as {typeof(TTarget)} - this object only supports {typeof(TTarget)}", nameof(target));
            }
        }

        /// <summary>
        /// Builds an expression from the specified target for the given <see cref="ICompileContext"/>
        ///
        /// OVerride this to implement the compilation for your target type.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target"/>.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided </param>
        protected abstract Expression Build(TTarget target, IExpressionCompileContext context, IExpressionCompiler compiler);

        /// <summary>
        /// Determines whether this instance can build an expression from the specified target.
        ///
        /// This base implementation simply checks that the type of <paramref name="targetType"/> is
        /// compatible with the type <typeparamref name="TTarget"/>.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        public override bool CanBuild(Type targetType)
        {
            return typeof(TTarget).IsAssignableFrom(targetType);
        }
    }
}
