// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Targets;

namespace Rezolver
{
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Registers the type <typeparamref name="TObject"/> to be created by an <see cref="IContainer"/> via constructor injection.
        /// The registration will auto-bind a constructor based on the services available in the <see cref="ITargetContainer"/> and
        /// <see cref="IContainer"/> available at the time <see cref="IContainer.Resolve(ResolveContext)"/> is first called.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that is to be constructed when resolved.  Also doubles up as the type to be
        /// used for the registration itself.</typeparam>
        /// <param name="targetContainer">The target container on which the registration is to be performed.</param>
        /// <param name="memberBinding">Optional - provides an explicit member injection behaviour to be used when creating the instance,
        /// if different from the behaviour configured via options on the <paramref name="targetContainer"/>.</param>
        /// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via
        /// the <see cref="Target.ForType{T}(IMemberBindingBehaviour)"/> static method and then registering it.</remarks>
        public static void RegisterType<TObject>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        {
            RegisterType(targetContainer, typeof(TObject), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers the type <typeparamref name="TObject"/> for the service type <typeparamref name="TService"/> to be created by
        /// an <see cref="IContainer"/> via constructor injection.
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
        /// the <see cref="Target.ForType{T}(IMemberBindingBehaviour)"/> static method and then registering it against
        /// the type <typeparamref name="TService"/>.</remarks>
        public static void RegisterType<TObject, TService>(this ITargetContainer targetContainer, IMemberBindingBehaviour memberBinding = null)
        where TObject : TService
        {
            RegisterType(targetContainer, typeof(TObject), serviceType: typeof(TService), memberBinding: memberBinding);
        }

        /// <summary>
        /// Registers the type <paramref name="objectType"/> (optionally for the service type <paramref name="serviceType"/>) to be
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
        /// the <see cref="Target.ForType(Type, IMemberBindingBehaviour)"/> static method and then registering it against
        /// the type <paramref name="serviceType"/> or <paramref name="objectType"/>.</remarks>
        public static void RegisterType(this ITargetContainer targetContainer, Type objectType, Type serviceType = null, IMemberBindingBehaviour memberBinding = null)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if(objectType == null) throw new ArgumentNullException(nameof(objectType));
            RegisterTypeInternal(targetContainer, objectType, serviceType, memberBinding);
        }

        /// <summary>
        /// Register the type <typeparamref name="TObject"/> to be created by the container via constructor injection, with an <see cref="IMemberBindingBehaviour"/>
        /// that's built from an <see cref="IMemberBindingBehaviourBuilder{TInstance}"/> that's configure by a callback you provide.
        /// </summary>
        /// <typeparam name="TObject">The type to be registered and created.</typeparam>
        /// <param name="targets">The target container on which the registration is to be performed.</param>
        /// <param name="configureMemberBinding">A callback that will be invoked with a new <see cref="IMemberBindingBehaviourBuilder{TInstance}"/>
        /// object that you can use to configure a custom member binding behaviour for the type <typeparamref name="TObject"/>.  The
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}.BuildBehaviour"/> method will be called after executing your callback to
        /// obtain the final <see cref="IMemberBindingBehaviour"/>.</param>
        public static void RegisterType<TObject>(this ITargetContainer targets, Action<IMemberBindingBehaviourBuilder<TObject>> configureMemberBinding)
        {
            RegisterType<TObject, TObject>(targets, configureMemberBinding);
        }

        /// <summary>
        /// Same as the <see cref="RegisterType{TObject}(ITargetContainer, Action{IMemberBindingBehaviourBuilder{TObject}})"/> method, except this
        /// creates a registration for <typeparamref name="TService"/> that will be implemented by instances of the type <typeparamref name="TObject"/>,
        /// created via constructor injection.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="targets">The target container on which the registration is to be performed.</param>
        /// <param name="configureMemberBinding">A callback that will be invoked with a new <see cref="IMemberBindingBehaviourBuilder{TInstance}"/>
        /// object that you can use to configure a custom member binding behaviour for the type <typeparamref name="TObject"/>.  The
        /// <see cref="IMemberBindingBehaviourBuilder{TInstance}.BuildBehaviour"/> method will be called after executing your callback to
        /// obtain the final <see cref="IMemberBindingBehaviour"/>.</param>
        public static void RegisterType<TObject, TService>(this ITargetContainer targets, Action<IMemberBindingBehaviourBuilder<TObject>> configureMemberBinding)
            where TObject : TService
        {
            var factory = MemberBindingBehaviour.For<TObject>();
            configureMemberBinding?.Invoke(factory);
            targets.RegisterType<TObject, TService>(factory.BuildBehaviour());
        }

        internal static void RegisterTypeInternal(ITargetContainer targetContainer, Type objectType, Type serviceType, IMemberBindingBehaviour memberBinding)
        {
            targetContainer.Register(Target.ForType(objectType, memberBinding), serviceType: serviceType);
        }
    }
}
