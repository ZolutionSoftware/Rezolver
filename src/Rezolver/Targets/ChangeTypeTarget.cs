// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver.Targets
{
    /// <summary>
    /// This target is specifically used for explicitly casting the result of one target to another type.
    /// Its use is rare, since the framework already caters for downcasting the result of targets to
    /// base types.
    /// </summary>
    /// <remarks>
    /// A valid use of this target is when you have a <see cref="SingletonTarget"/> registered against one
    /// type, and you want the same singleton (backed by the same instance) to server another type.
    ///
    /// In this case, instead of registering the same singleton target multiple times, you can register it once
    /// for its primary type, then register one of these for the other type, with a <see cref="ResolvedTarget"/>
    /// as its inner target.
    ///
    /// When creating this target, the <see cref="ITarget.DeclaredType"/> of the <see cref="InnerTarget"/> must
    /// be able to cast up or down to the <see cref="DeclaredType"/> of this target.
    /// </remarks>
    public class ChangeTypeTarget : TargetBase
    {
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
        /// Always returns the target type that was passed in the <see cref="ChangeTypeTarget.ChangeTypeTarget(ITarget, Type)"/> constructor.
        /// </summary>
        public override Type DeclaredType { get; }

        /// <summary>
        /// The target whose type will be changed to <see cref="DeclaredType"/>.
        /// </summary>
        public ITarget InnerTarget { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ChangeTypeTarget"/> class.
        /// </summary>
        /// <param name="innerTarget">Required.  See <see cref="InnerTarget"/></param>
        /// <param name="targetType">Required.  See <see cref="DeclaredType"/></param>
        public ChangeTypeTarget(ITarget innerTarget, Type targetType)
        {
            innerTarget.MustNotBeNull(nameof(innerTarget));
            targetType.MustNotBeNull(nameof(targetType));

            // to check validity of the type change, we can't use innerTarget.SupportsType, because that's a different
            // idea - that method returns true if it can build an instance of that type.
            // What we need here is simply a check that the two types can be cast either up or down to each other - which
            // should be a simple IsAssignableFrom check in either direction.
            if (!targetType.IsAssignableFrom(innerTarget.DeclaredType)
                && !innerTarget.DeclaredType.IsAssignableFrom(targetType))
            {
                throw new ArgumentException($"The type {targetType} is not compatible with the target's DeclaredType {innerTarget.DeclaredType}", nameof(targetType));
            }

            InnerTarget = innerTarget;
            DeclaredType = targetType;
        }
    }
}
