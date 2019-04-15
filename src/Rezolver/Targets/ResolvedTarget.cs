// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
    /// <summary>
    /// Represents a target that is rezolved statically at compile time via the <see cref="ICompileContext"/>, or dynamically
    /// (at 'resolve time') from the <see cref="IContainer"/> that is attached to the current <see cref="ResolveContext"/> when
    /// <see cref="IContainer.Resolve(ResolveContext)"/> is called.
    ///
    /// This is the most common way that we bind constructor parameters, for example - i.e. 'I want an
    /// <c>IService</c> instance - go get it'.
    /// </summary>
    /// <remarks>Represents an object that will be resolved from the container when its <see cref="ICompiledTarget"/> is executed,
    /// or when the target is used perhaps by another <see cref="ITarget"/> (e.g. - a <see cref="ConstructorTarget"/> with a
    /// constructor parameter bound to one of these).
    ///
    /// So, in essence, a <see cref="ResolvedTarget"/> represents an automatic call to a container's
    /// <see cref="IContainer.Resolve(ResolveContext)"/> method, for the <see cref="DeclaredType"/>.
    ///
    /// In practise - an <see cref="ITargetCompiler"/> might take advantage of the fact that, during compilation, targets
    /// can be discovered directly from the <see cref="ICompileContext"/> that is passed to
    /// <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> by leveraging its <see cref="ITargetContainer"/>
    /// implementation.
    ///
    /// Indeed - the expressions compiler uses this to avoid repeated recursion into the Resolve method of the container, instead
    /// choosing to compile all the expressions for all targets required for an operation into one dynamically built method -
    /// which results in very fast execution times for all resolve operations.
    ///
    /// Not only this, but the behaviour can be extended still further by realising that a <see cref="ResolveContext.Container"/>
    /// on which a resolve operation is invoked might not be the same container for which this <see cref="ResolvedTarget"/> was
    /// first compiled.  In this case - it's possible that the other container has alternative registrations for a given service
    /// type which the application expects to take precedence over those which were originally resolved when compilation took
    /// place.  The expressions compiler, again, detects this - allowing compiled code from a root container to detect an
    /// 'overriding' container and to dynamically resolve a different dependency in this situation.
    /// </remarks>
    public class ResolvedTarget : TargetBase
    {
        /// <summary>
        /// The type that is to be resolved from the container at resolve-time.
        /// </summary>
        public override Type DeclaredType { get; }

        /// <summary>
        /// Always returns <see cref="ScopeBehaviour.None"/>
        /// </summary>
        /// <value>The scope behaviour.</value>
        public override ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.None;
            }
        }

        /// <summary>
        /// Gets the target that this <see cref="ResolvedTarget"/> will fallback to if a satisfactory target cannot be found
        /// at compile time or resolve-time.
        /// </summary>
        /// <remarks>The <see cref="ITarget.UseFallback"/> property is also used to determine whether this will be
        /// used.  If the target resolved from the <see cref="ICompileContext"/> has its <see cref="ITarget.UseFallback"/>
        /// property set to true, and this property is non-null for this target, then this target will be used.
        ///
        /// Note also that extension containers such as <see cref="OverridingContainer"/> also have the ability to override
        /// the use of this fallback if they successfully resolve the type.
        /// </remarks>
        public ITarget FallbackTarget { get; }

        /// <summary>
        /// Creates a new <see cref="ResolvedTarget"/> for the given <paramref name="type"/> which will attempt to
        /// resolve a value at compile time and/or resolve-time and, if it can't, will either use the <paramref name="fallbackTarget"/>
        /// or will throw an exception.
        /// </summary>
        /// <param name="type">Required.  The type to be resolved</param>
        /// <param name="fallbackTarget">Optional.  The target to be used if the value cannot be resolved at either compile time or
        /// resolve-time.  An <see cref="ArgumentException"/> is thrown if this target's <see cref="ITarget.SupportsType(Type)"/>
        /// function returns <c>false</c> when called with the <paramref name="type"/>.</param>
        public ResolvedTarget(Type type, ITarget fallbackTarget = null)
        {
            if(type == null) throw new ArgumentNullException(nameof(type));
            if (fallbackTarget != null && !fallbackTarget.SupportsType(type))
            {
                throw new ArgumentException($"The fallback target must support the passed type {type}", nameof(fallbackTarget));
            }

            DeclaredType = type;
            FallbackTarget = fallbackTarget;
        }

        /// <summary>
        /// Attempts to obtain the target that this <see cref="ResolvedTarget"/> resolves to for the given <see cref="ICompileContext"/>.
        ///
        /// This function should be used by <see cref="ITargetCompiler"/> implementations when producing the <see cref="ICompiledTarget"/>
        /// for this instance, who wish to perform some form of up-front optimisations.
        /// </summary>
        /// <param name="context">The context from which a target is to be resolved.</param>
        /// <returns>The target resolved by this target - could be the <see cref="FallbackTarget"/>, could be null.</returns>
        /// <remarks>The target that is returned depends both on the <paramref name="context"/> passed and also whether
        /// a <see cref="FallbackTarget"/> has been provided to this target.
        /// </remarks>
        public virtual ITarget Bind(ICompileContext context)
        {
            if(context == null) throw new ArgumentNullException(nameof(context));

            var fromContext = context.Fetch(DeclaredType);
            if (fromContext == null)
            {
                return FallbackTarget; // might still be null of course
            }
            else if (fromContext.UseFallback)
            {
                return FallbackTarget ?? fromContext;
            }

            return fromContext;
        }
    }
}