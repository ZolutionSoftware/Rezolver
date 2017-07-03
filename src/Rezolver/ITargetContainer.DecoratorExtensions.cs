// Copyright (c) Zolution Software Ltd. All rights reserved.
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
			targetContainer.MustNotBeNull(nameof(targetContainer));
            RegisterDecoratorInternal(targetContainer, typeof(TDecorator), typeof(TDecorated));
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
            targetContainer.MustNotBeNull(nameof(targetContainer));
            decoratorType.MustNotBeNull(nameof(decoratorType));
            decoratedType.MustNotBeNull(nameof(decoratedType));
            RegisterDecoratorInternal(targetContainer, decoratorType, decoratedType);
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
    }
}
