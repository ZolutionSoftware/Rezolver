// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Targets;

namespace Rezolver
{
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Registers an instance to be used when resolving a particular service type via the <see cref="ObjectTarget"/>
        /// target.
        /// </summary>
        /// <typeparam name="T">Type of the object - will be used as the service type for registration if
        /// <paramref name="serviceType"/> is not provied.</typeparam>
        /// <param name="targetContainer">The target container which will receive the registration.</param>
        /// <param name="obj">The instance that will be resolved when the service type is requested.</param>
        /// <param name="declaredType">Type to be set as the <see cref="ITarget.DeclaredType"/> of the <see cref="ObjectTarget"/> that is
        /// created for <paramref name="obj"/>, if different from <typeparamref name="T"/>.</param>
        /// <param name="serviceType">The service type against which this object is to be registered, if different
        /// from <typeparamref name="T"/>.</param>
        /// <param name="scopeBehaviour">Sets the <see cref="ITarget.ScopeBehaviour"/> for the <see cref="ObjectTarget"/> that's created</param>
        /// <remarks>By default, the <see cref="ITarget.DeclaredType"/> of the <see cref="ObjectTarget"/> that is created is
        /// fixed to <typeparamref name="T"/> - use the <paramref name="declaredType"/> parameter to override this if <typeparamref name="T"/>
        /// is a less specific type than the one for which you wish to create a registration.</remarks>
        public static void RegisterObject<T>(this ITargetContainer targetContainer, T obj, Type declaredType = null, Type serviceType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            targetContainer.Register(Target.ForObject(obj, declaredType: declaredType ?? typeof(T), scopeBehaviour: scopeBehaviour), serviceType ?? declaredType ?? typeof(T));
        }

        /// <summary>
        /// Registers an instance to be used when resolve a particular service type via the <see cref="ObjectTarget"/>
        /// </summary>
        /// <param name="targetContainer">The target container which will receive the registration.</param>
        /// <param name="obj">Required, but can be <c>null</c>.  The instance that will be resolved when the service type is requested.</param>
        /// <param name="declaredType">Type to be set as the <see cref="ITarget.DeclaredType"/> of the <see cref="ObjectTarget"/>
        /// that is created for <paramref name="obj"/>, if different from the object's type.</param>
        /// <param name="serviceType">The service type against which this object is to be registered, if different
        /// from <paramref name="declaredType"/> (or the object's type).</param>
        /// <param name="scopeBehaviour">Sets the <see cref="ITarget.ScopeBehaviour"/> for the <see cref="ObjectTarget"/> that's created</param>
        /// <remarks><c>null</c> objects are implicitly treated as <see cref="System.Object"/> if <paramref name="declaredType"/> is not passed.</remarks>
        public static void RegisterObject(this ITargetContainer targetContainer, object obj, Type declaredType = null, Type serviceType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            targetContainer.Register(Target.ForObject(obj, declaredType ?? obj?.GetType(), scopeBehaviour: scopeBehaviour), serviceType ?? declaredType ?? obj?.GetType());
        }
    }
}
