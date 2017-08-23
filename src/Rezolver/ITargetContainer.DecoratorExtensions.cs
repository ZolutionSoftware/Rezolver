﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for <see cref="ITargetContainer"/> which simplify the registration of decorators (via the
	/// <see cref="DecoratingTargetContainer"/> pseudo-target)
	/// </summary>
    public static class DecoratorTargetContainerExtensions
    {
		/// <summary>
		/// Registers a decorator container which will cause all instances of <typeparamref name="TDecorated"/> to be decorated with
		/// the type <typeparamref name="TDecorator"/>.
		/// 
		/// Any existing registrations for <typeparamref name="TDecorated"/> will be decorated correctly, and subsequent registrations 
		/// of <typeparamref name="TDecorated"/> will also be decorated as expected.
		/// </summary>
		/// <typeparam name="TDecorator">The type to be used as the decorator implementation</typeparam>
		/// <typeparam name="TDecorated">The type which will be decorated by <typeparamref name="TDecorator"/>.</typeparam>
		/// <param name="targetContainer">The container into which the decorator will be registered.</param>
		public static void RegisterDecorator<TDecorator, TDecorated>(this ITargetContainer targetContainer)
		{
            RegisterDecoratorInternal(targetContainer ?? throw new ArgumentNullException(nameof(targetContainer)),
                typeof(TDecorator), 
                typeof(TDecorated));
		}

        /// <summary>
        /// Registers a decorator container which will cause all instances of the type <typeparamref name="TDecorated"/> produced by the 
        /// container to be intercepted and replaced by the result of calling the passed <paramref name="decoratorDelegate"/> with the
        /// original instance.
        /// </summary>
        /// <typeparam name="TDecorated">The type of object whose creation is being decorated by the delegate.</typeparam>
        /// <param name="targetContainer">The container into which the decorator will be registered.</param>
        /// <param name="decoratorDelegate">The delegate to be executed every time an instance of <typeparamref name="TDecorated"/>
        /// is produced by the container, and whose result will be used in place of the original object (which is fed into the delegate).</param>
        /// <remarks>
        /// Whilst this overload uses the term 'Decorator' in its name, it is of course entirely possible that the delegate won't actually
        /// create a decorating instance for the input object.
        /// 
        /// As a result, it's better to think of this as decorating Rezolver's own process of getting an object, which may or may not result in
        /// a decorated instance - depending on what the delegate actually does.
        /// 
        /// What this *does* allow, however, is decorating objects which otherwise can't be decorated by constructor injection - e.g. Arrays,
        /// delegate types, primitive objects (e.g. <see cref="int"/>) and so on.
        /// </remarks>
        public static void RegisterDecoratorDelegate<TDecorated>(this ITargetContainer targetContainer, Func<TDecorated, TDecorated> decoratorDelegate)
        {
            RegisterDecoratorDelegateInternal(targetContainer ?? throw new ArgumentNullException(nameof(targetContainer)),
                decoratorDelegate ?? throw new ArgumentNullException(nameof(decoratorDelegate)),
                typeof(TDecorated));
        }

        public static void RegisterDecoratorDelegate(this ITargetContainer targetContainer, Delegate decoratorDelegate, Type decoratedType)
        {
            RegisterDecoratorDelegateInternal(targetContainer ?? throw new ArgumentNullException(nameof(targetContainer)),
                decoratorDelegate ?? throw new ArgumentNullException(nameof(decoratorDelegate)),
                decoratedType ?? throw new ArgumentNullException(nameof(decoratedType)));
        }

        /// <summary>
        /// Registers a decorator container which will cause all instances of <paramref name="decoratedType" /> to be decorated with
        /// the type <paramref name="decoratorType" />.
        /// 
        /// Any existing registrations for <paramref name="decoratedType" /> will be decorated correctly, and subsequent registrations 
        /// of <paramref name="decoratedType" /> will also be decorated as expected.
        /// </summary>
        /// <param name="targetContainer">The container into which the decorator will be registered.</param>
        /// <param name="decoratorType">The type to be used as the decorator implementation</param>
        /// <param name="decoratedType">The type which will be decorated by <paramref name="decoratorType" />.</param>
        public static void RegisterDecorator(this ITargetContainer targetContainer, Type decoratorType, Type decoratedType)
        {
            RegisterDecoratorInternal(targetContainer ?? throw new ArgumentNullException(nameof(targetContainer)),
                decoratorType ?? throw new ArgumentNullException(nameof(decoratorType)),
                decoratedType ?? throw new ArgumentNullException(nameof(decoratedType)));
        }

        private static void RegisterDecoratorInternal(ITargetContainer targetContainer, Type decoratorType, Type decoratedType)
        {
            //decorators for generics are always registered against the open generic type.
            //the decorator, however, will only create a decorated target when the type requested
            //equals the type it's decorating; and if that happens to be the open generic, then it'll
            //be all types.
            Type registerType = decoratedType;
            if (TypeHelpers.IsGenericType(decoratedType))
            {
                if (!TypeHelpers.IsGenericTypeDefinition(decoratedType))
                    registerType = decoratedType.GetGenericTypeDefinition();
            }
            targetContainer.RegisterContainer(registerType, new DecoratingTargetContainer(targetContainer, decoratorType, decoratedType));
        }

        private static void RegisterDecoratorDelegateInternal(ITargetContainer targetContainer, Delegate decoratorDelegate, Type decoratedType)
        {
            targetContainer.RegisterContainer(decoratedType, new DecoratingTargetContainer(targetContainer, Target.ForDelegate(decoratorDelegate, decoratedType), decoratedType));
        }
    }
}
