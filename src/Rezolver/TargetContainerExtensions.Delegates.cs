﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Reflection;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Extension methods for ITargetContainer designed to simplify the registration of <see cref="DelegateTarget"/> and its
    /// numerous generic variants.
    /// </summary>
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Constructs a <see cref="DelegateTarget"/> from the passed factory delegate (optionally with the given <paramref name="declaredType"/>,
        /// <paramref name="scopeBehaviour"/> and <paramref name="scopePreference"/>) and registers it in the target container.
        /// </summary>
        /// <param name="targetContainer">The target container in which the new target is to registered</param>
        /// <param name="factory">The factory delegate that is to be executed by the <see cref="DelegateTarget"/> that is created.</param>
        /// <param name="declaredType">Optional - if provided, then it overrides the <see cref="ITarget.DeclaredType"/> of the <see cref="DelegateTarget"/>
        /// that is created which, in turn, will change the type against which the target will be registered in the target container.  If null, then
        /// the return type of the factory will be used.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="ContainerScope" />.  The default is <see cref="ScopeBehaviour.None" /> - which means 
        /// no disposal will take place.  Be careful with changing this - if the delegate produces new instances each time it's used, then 
        /// <see cref="ScopeBehaviour.Implicit"/> is suitable; if not, then only <see cref="ScopeBehaviour.None"/> or <see cref="ScopeBehaviour.Explicit"/>
        /// are suitable.</param>
        /// <param name="scopePreference">Optional.  If <paramref name="scopeBehaviour"/> is not <see cref="ScopeBehaviour.None"/>, then this controls the
        /// type of scope in which the instance should be tracked.  Defaults to <see cref="ScopePreference.Current"/>.  <see cref="ScopePreference.Root"/>
        /// should be used if the result of the delegate is effectively a singleton.</param>
        public static void RegisterDelegate(
            this ITargetContainer targetContainer, 
            Delegate factory, 
            Type declaredType = null, 
            ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit,
            ScopePreference scopePreference = ScopePreference.Current)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if(factory == null) throw new ArgumentNullException(nameof(factory));

            ITarget toRegister = null;
            if (factory.GetMethodInfo().GetParameters()?.Length > 0)
            {
                toRegister = new DelegateTarget(factory, declaredType, scopeBehaviour: scopeBehaviour, scopePreference: scopePreference);
            }
            else
            {
                toRegister = new NullaryDelegateTarget(factory, declaredType, scopeBehaviour: scopeBehaviour, scopePreference: scopePreference);
            }

            targetContainer.Register(toRegister);
        }
    }
}
