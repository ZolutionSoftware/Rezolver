// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Targets;

namespace Rezolver
{
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Registers an explicitly scoped instance of <typeparamref name="TObject"/> to be created by an <see cref="IContainer"/> via
        /// constructor injection.
        /// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and
        /// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(ResolveContext)"/> is first called.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that is to be constructed when resolved.  Also doubles up as the type to be
        /// used for the registration itself.</typeparam>
        /// <param name="targetContainer">The target container on which the registration is to be performed.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
        /// the <see cref="Target.ForType{T}(IMemberBindingBehaviour)"/> static method, wrapping it with a
        /// <see cref="ScopedTarget"/> and registering it.</remarks>
        public static void RegisterScoped<TObject>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        {
            RegisterScoped(targetContainer, typeof(TObject), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers an explicitly scoped instance of <typeparamref name="TObject"/> for the service type <typeparamref name="TService"/>
        /// to be created by an <see cref="IContainer"/> via constructor injection.
        /// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and
        /// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(ResolveContext)"/> is first called.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that is to be constructed when resolved.</typeparam>
        /// <typeparam name="TService">The type against which the registration will be performed.  <typeparamref name="TObject"/> must be
        /// compatible with this type.</typeparam>
        /// <param name="targetContainer">The target container on which the registration is to be performed.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
        /// the <see cref="Target.ForType{T}(IMemberBindingBehaviour)"/> static method, wrapping it with a
        /// <see cref="ScopedTarget"/> and then registering it against
        /// the type <typeparamref name="TService"/>.</remarks>
        public static void RegisterScoped<TObject, TService>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        {
            RegisterScoped(targetContainer, typeof(TObject), typeof(TService), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers an explicitly instance of <paramref name="objectType"/> (optionally for the service type <paramref name="serviceType"/>) to be
        /// created by an <see cref="IContainer"/> via constructor injection.
        /// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and
        /// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(ResolveContext)"/> is first called.
        /// </summary>
        /// <param name="targetContainer">The target container on which the registration is to be performed.</param>
        /// <param name="objectType">The type of the object that is to be constructed when resolved.</param>
        /// <param name="serviceType">Optional.  The type against which the registration will be performed, if different from
        /// <paramref name="objectType"/>.  <paramref name="objectType"/> must be compatible with this type, if it's provided.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
        /// the <see cref="Target.ForType(Type, IMemberBindingBehaviour)"/> static method, wrapping it with a <see cref="ScopedTarget"/>
        /// and then registering it against the type <paramref name="serviceType"/> or <paramref name="objectType"/>.</remarks>
        public static void RegisterScoped(this ITargetContainer targetContainer, Type objectType, Type serviceType = null, IMemberBindingBehaviour memberBinding = null)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if(objectType == null) throw new ArgumentNullException(nameof(objectType));

            RegisterScopedInternal(targetContainer, objectType, serviceType, memberBinding);
        }

        internal static void RegisterScopedInternal(ITargetContainer targetContainer, Type objectType, Type serviceType, IMemberBindingBehaviour memberBinding)
        {
            targetContainer.Register(Target.ForType(objectType, memberBinding).Scoped(), serviceType: serviceType);
        }
    }
}
