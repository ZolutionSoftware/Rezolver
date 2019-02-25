// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Extension methods for the <see cref="ITargetContainer"/> interface which simplify the registration of singletons.
    /// </summary>
    public static class SingletonTargetContainerExtensions
    {
        /// <summary>
        /// Registers the type <typeparamref name="TObject"/> as a singleton (<see cref="SingletonTarget"/>) in the target container.
        ///
        /// The instance will be built automatically with constructor injection by leveraging either the <see cref="ConstructorTarget"/> or
        /// <see cref="GenericConstructorTarget"/>, depending on whether <typeparamref name="TObject"/> is a generic type or not.
        /// </summary>
        /// <typeparam name="TObject">The type to be created, and the type against which the registration will be made</typeparam>
        /// <param name="targetContainer">The container on which the registrations will be made.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        public static void RegisterSingleton<TObject>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        {
            RegisterSingleton(targetContainer, typeof(TObject), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers the type <typeparamref name="TObject"/> as a singleton (<see cref="SingletonTarget"/>) in the target container
        /// for the service type <typeparamref name="TService"/>.
        ///
        /// The instance will be built automatically with constructor injection by leveraging either the <see cref="ConstructorTarget"/> or
        /// <see cref="GenericConstructorTarget"/>, depending on whether <typeparamref name="TObject"/> is a generic type or not.
        /// </summary>
        /// <typeparam name="TObject">The type of object to be created.</typeparam>
        /// <typeparam name="TService">The type against which the target will be registered in the <paramref name="targetContainer"/></typeparam>
        /// <param name="targetContainer">The container on which the registrations will be made.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        public static void RegisterSingleton<TObject, TService>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        {
            RegisterSingleton(targetContainer, typeof(TObject), typeof(TService), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers the type <paramref name="objectType"/> as a singleton (<see cref="SingletonTarget"/>) in the target container,
        /// either for the same service type, or for the service type passed to the <paramref name="serviceType"/> parameter (if provided).
        ///
        /// The instance will be built automatically with constructor injection by leveraging either the <see cref="ConstructorTarget"/> or
        /// <see cref="GenericConstructorTarget"/>, depending on whether <paramref name="objectType"/> is a generic type or not.
        /// </summary>
        /// <param name="targetContainer">The container on which the registrations will be made.</param>
        /// <param name="objectType">Required.  The type of object to be created.</param>
        /// <param name="serviceType">Optional.  The type against which the target will be registered in the <paramref name="targetContainer"/></param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        public static void RegisterSingleton(this ITargetContainer targetContainer, Type objectType, Type serviceType = null, IMemberBindingBehaviour memberBinding = null)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if(objectType == null) throw new ArgumentNullException(nameof(objectType));

            RegisterSingletonInternal(targetContainer, objectType, serviceType, memberBinding);
        }

        internal static void RegisterSingletonInternal(ITargetContainer builder, Type objectType, Type serviceType, IMemberBindingBehaviour memberBinding)
        {
            builder.Register(Target.ForType(objectType, memberBinding).Singleton(), serviceType: serviceType);
        }
    }
}
