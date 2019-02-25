// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Extension methods for ITargetContainer designed to simplify the registration of <see cref="DelegateTarget"/> and its
    /// numerous generic variants.
    /// </summary>
    public static partial class DelegateTargetContainerExtensions
    {
        /// <summary>
        /// Constructs a <see cref="DelegateTarget"/> from the passed factory delegate (optionally with the given <paramref name="declaredType"/>)
        /// and registers it in the target container.
        /// </summary>
        /// <param name="targetContainer">The target container in which the new target is to registered</param>
        /// <param name="factory">The factory delegate that is to be executed by the <see cref="DelegateTarget"/> that is created.</param>
        /// <param name="declaredType">Optional - if provided, then it overrides the <see cref="ITarget.DeclaredType"/> of the <see cref="DelegateTarget"/>
        /// that is created which, in turn, will change the type against which the target will be registered in the target container.  If null, then
        /// the return type of the factory will be used.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the factory delegate will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
        public static void RegisterDelegate(this ITargetContainer targetContainer, Delegate factory, Type declaredType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if(factory == null) throw new ArgumentNullException(nameof(factory));

            ITarget toRegister = null;
            if (factory.GetMethodInfo().GetParameters()?.Length > 0)
            {
                toRegister = new DelegateTarget(factory, declaredType);
            }
            else
            {
                toRegister = new NullaryDelegateTarget(factory, declaredType);
            }

            if (scopeBehaviour == ScopeBehaviour.Explicit)
            {
                toRegister = toRegister.Scoped();
            }
            else if (scopeBehaviour == ScopeBehaviour.None)
            {
                toRegister = toRegister.Unscoped();
            }

            targetContainer.Register(toRegister);
        }
    }
}
