// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;

namespace Rezolver.Compilation
{
    /// <summary>
    /// Tracks state and provides services during the compilation phase in which <see cref="ITarget"/> targets
    /// are compiled into <see cref="Func{ResolveContext, TResult}"/> factories.
    ///
    /// Instances of this interface are created by calling the
    /// <see cref="ITargetCompiler.CreateContext(ResolveContext, ITargetContainer)"/> method of the <see cref="ITargetCompiler"/>
    /// which is registered in a container.
    /// </summary>
    /// <seealso cref="Rezolver.ITargetContainer" />
    /// <remarks>
    /// Implementations must also implement the <see cref="ITargetContainer"/> so the compile context can be used for
    /// dependency lookups during compilation time.  Indeed, if you are developing your own compilation component (possibly
    /// for a custom <see cref="ITarget"/> implementation) and need to resolve any dependencies from an <see cref="ITargetContainer"/>
    /// during compilation, it should be done through the context's implementation of <see cref="ITargetContainer"/>.</remarks>
    public interface ICompileContext : ITargetContainer
    {
        /// <summary>
        /// Gets the parent context from which this context was created, if applicable.
        /// </summary>
        /// <value>The parent context.</value>
        ICompileContext ParentContext { get; }

        /// <summary>
        /// If not <c>null</c> then this overrides the <see cref="ITarget.ScopeBehaviour"/> of the <see cref="ITarget"/>
        /// that's currently being compiled.
        ///
        /// Note that this is not automatically inherited from one context to another
        /// </summary>
        ScopeBehaviour? ScopeBehaviourOverride { get; }

        /// <summary>
        /// Same as <see cref="ScopeBehaviourOverride"/> but this is used to override currently compiling target's
        /// <see cref="ITarget.ScopePreference"/>.
        ///
        /// Typically used, for example, when a singleton is compiling other targets.
        /// </summary>
        ScopePreference? ScopePreferenceOverride { get; }

        /// <summary>
        /// Any factory built for a <see cref="ITarget"/> with this context should target this type.
        /// If null, then the <see cref="ITarget.DeclaredType"/> of the target being compiled should be used.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets the stack entries for all the targets that are being compiled.
        /// </summary>
        /// <value>The compile stack.</value>
        IEnumerable<CompileStackEntry> CompileStack { get; }

        /// <summary>
        /// The context for which this compilation context was created.
        ///
        /// Provides access to the current container, scope and more besides
        /// </summary>
        ResolveContext ResolveContext { get; }

        /// <summary>
        /// Creates a new child context from this one, except the <see cref="TargetType"/> and
        /// <see cref="ScopeBehaviourOverride"/> properties can be overriden if required.
        ///
        /// Implementations must make sure that the <see cref="ScopeBehaviourOverride"/>
        /// is *never* inherited.
        /// </summary>
        /// <param name="targetType">Optional.  The type for which the target is to be compiled, if different from this context's <see cref="TargetType"/>.</param>
        /// <param name="scopeBehaviourOverride">Value for the <see cref="ScopeBehaviourOverride"/> to be used for the target that is compiled with the new context.
        /// This is never inherited automatically from one context to another.</param>
        /// <param name="scopePreferenceOverride">Sets the <see cref="ScopePreferenceOverride"/>.  As soon as this is set on one context, it is automatically
        /// inherited by all its child contexts (i.e. you cannot null it)</param>
        /// <returns>A new <see cref="ICompileContext" />.</returns>
        ICompileContext NewContext(Type targetType = null,
            ScopeBehaviour? scopeBehaviourOverride = null,
            ScopePreference? scopePreferenceOverride = null);

        /// <summary>
        /// Pops the compile stack, returning the entry that was popped.
        /// </summary>
        CompileStackEntry PopCompileStack();

        /// <summary>
        /// Pushes the passed target on to the compile stack if it's not already on it for the same <see cref="TargetType" />
        /// Compilers should consult the return value and abort compilation if it's <c>true</c> - since that implies a cyclic
        /// dependency graph.
        /// </summary>
        /// <param name="toCompile">To compile.</param>
        /// <param name="targetType">The type for which the target is being compiled, if different from <see cref="ITarget.DeclaredType"/></param>
        /// <remarks>Targets can appear on the compilation stack more than once for different types, since the factory
        /// produced for a target for one type can be different than it is for another.  Ultimately, if a target does in fact have a
        /// cyclic dependency graph, then this method will detect that.</remarks>
        bool PushCompileStack(ITarget toCompile, Type targetType = null);
    }
}

