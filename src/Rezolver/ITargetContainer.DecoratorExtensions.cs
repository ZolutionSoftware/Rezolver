using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for <see cref="ITargetContainerOwner"/> which simplify the registration of decorators (via the
	/// <see cref="DecoratorTarget"/> pseudo-target)
	/// </summary>
	/// <remarks>Note: The decoration functionality provided by the framework is only possible on 
	/// <see cref="ITargetContainer"/> implementations which also implement the <see cref="ITargetContainerOwner"/> interface.
	/// 
	/// All the main target container types you'll use in your application (<see cref="TargetContainer"/> and <see cref="ChildTargetContainer"/>)
	/// do support this interface.</remarks>
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
		/// <param name="targetContainerOwner">The container into which the decorator will be registered.</param>
		public static void RegisterDecorator<TDecorator, TDecorated>(this ITargetContainerOwner targetContainerOwner)
		{
			targetContainerOwner.MustNotBeNull(nameof(targetContainerOwner));
			targetContainerOwner.RegisterContainer(typeof(TDecorated), new DecoratorTarget(typeof(TDecorator), typeof(TDecorated)));
		}

		/// <summary>
		/// Registers a decorator container which will cause all instances of <paramref name="decoratedType" /> to be decorated with
		/// the type <paramref name="decoratorType" />.
		/// 
		/// Any existing registrations for <paramref name="decoratedType" /> will be decorated correctly, and subsequent registrations 
		/// of <paramref name="decoratedType" /> will also be decorated as expected.
		/// </summary>
		/// <param name="decoratorType">The type to be used as the decorator implementation</param>
		/// <param name="decoratedType">The type which will be decorated by <paramref name="decoratorType" />.</param>
		/// <param name="targetContainerOwner">The container into which the decorator will be registered.</param>
		public static void RegisterDecorator(this ITargetContainerOwner targetContainerOwner, Type decoratorType, Type decoratedType)
		{
			targetContainerOwner.MustNotBeNull(nameof(targetContainerOwner));
			decoratorType.MustNotBeNull(nameof(decoratorType));
			decoratedType.MustNotBeNull(nameof(decoratedType));

			targetContainerOwner.RegisterContainer(decoratedType, new DecoratorTarget(decoratorType, decoratedType));
		}
	}
}
