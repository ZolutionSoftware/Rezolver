// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
    /// <summary>
    /// Delegate type that is passed to the <see cref="DecoratorTarget"/> on construction to obtain
    /// a reference to an <see cref="ITarget"/> which, when compiled and executed, will produce an instance
    /// of the decorator object.
    /// </summary>
    /// <param name="decoratedTarget">The target whose result is to be decorated</param>
    /// <param name="decoratedType">The type being decorated.</param>
    /// <returns>An <see cref="ITarget"/> which, when compiled and executed correctly, will produce an instance of
    /// the decorator, with the instance produced by the <paramref name="decoratedTarget" /> as the decorated object
    /// inside of it.</returns>
    public delegate ITarget DecoratingTargetFactory(ITarget decoratedTarget, Type decoratedType);

	/// <summary>
	/// Represents the action of implementing a common <see cref="DecoratedType"/> by decorating one instance 
	/// (produced by <see cref="DecoratedTarget"/>) with another (<see cref="InnerTarget"/>).
	/// 
	/// NOTE - You shouldn't register or otherwise create instances of this target unless you absolutely 
	/// know what you're doing.  Rather, decorators should be registered using the extension method
	/// <see cref="DecoratorTargetContainerExtensions.RegisterDecorator{TDecorator, TDecorated}(ITargetContainer)"/>
	/// or its non-generic alternative because the target needs a <see cref="DecoratingTargetContainer"/>
	/// to work properly (the creation of which is automatically handled by these extension methods).
	/// </summary>
	/// <seealso cref="TargetBase" />
	public class DecoratorTarget : TargetBase
	{
        /// <summary>
        /// The type of object returned by the decorator target
        /// </summary>
        public override Type DeclaredType => InnerTarget.DeclaredType;

		/// <summary>
		/// Gets the target which will create an instance of the decorator
		/// </summary>
		public ITarget InnerTarget { get; }
		/// <summary>
		/// Gets the target whose instance will be wrapped (decorated) by the one produced by 
		/// <see cref="InnerTarget"/>.
		/// </summary>
		public ITarget DecoratedTarget { get; }
		/// <summary>
		/// Gets the underlying type (e.g. a common service interface or base) that is being implemented
		/// by decoration.
		/// </summary>
		public Type DecoratedType { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="DecoratorTarget"/> type when the target that create the decorator is already known.
        /// </summary>
        /// <param name="decoratorTarget"></param>
        /// <param name="decoratedTarget"></param>
        /// <param name="decoratedType"></param>
        public DecoratorTarget(ITarget decoratorTarget, ITarget decoratedTarget, Type decoratedType)
        {
            decoratorTarget.MustNotBeNull(nameof(decoratorTarget));
            decoratedTarget.MustNotBeNull(nameof(decoratedTarget));
            decoratedType.MustNotBeNull(nameof(decoratedType));

            if (!decoratorTarget.SupportsType(decoratedType))
                throw new ArgumentException($"The type passed ({decoratedType}) is not compatible with the decoratorTarget {decoratorTarget}", nameof(decoratedType));

            if(!decoratedTarget.SupportsType(decoratedType))
                throw new ArgumentException($"The type passed ({decoratedType}) is not compatible with the decoratedTarget {decoratedTarget}", nameof(decoratedType));

            DecoratedTarget = decoratedTarget;
            DecoratedType = decoratedType;
            InnerTarget = decoratorTarget;
        }

		/// <summary>
		/// Overrides <see cref="TargetBase.SupportsType(Type)"/> to forward the call to <see cref="InnerTarget"/>.
		/// </summary>
		/// <param name="type">The type which is to be checked.</param>
		/// <returns><c>true</c> if the type is compatible with the object created by <see cref="InnerTarget"/>, <c>false</c>
		/// if not.</returns>
		public override bool SupportsType(Type type)
		{
			return InnerTarget.SupportsType(type);
		}
	}
}
