// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
	/// <summary>
	/// Represents the action of implementing a common <see cref="DecoratedType"/> by decorating one instance 
	/// (produced by <see cref="DecoratedTarget"/>) with another (<see cref="Target"/>, which will create an 
	/// instance of <see cref="DecoratorType"/>).
	/// 
	/// NOTE - You shouldn't register or otherwise create instances of this target unless you absolutely 
	/// know what you're doing.  Rather, decorators should be registered using the extension method
	/// <see cref="DecoratorTargetContainerExtensions.RegisterDecorator{TDecorator, TDecorated}(ITargetContainerOwner)"/>
	/// or its non-generic alternative because the target needs a <see cref="DecoratingTargetContainer"/>
	/// to work properly (the creation of which is automatically handled by these extension methods).
	/// </summary>
	/// <seealso cref="TargetBase" />
	public class DecoratorTarget : TargetBase
	{
		/// <summary>
		/// Always returns <see cref="DecoratorType"/>
		/// </summary>
		public override Type DeclaredType
		{
			get
			{
				return DecoratorType;
			}
		}

		/// <summary>
		/// Gets the type which is decorating the instance produced by the 
		/// <see cref="DecoratedTarget"/> for the common service type <see cref="DecoratedType"/>
		/// </summary>
		public Type DecoratorType { get; }
		/// <summary>
		/// Gets the target which will create an instance of the <see cref="DecoratorType"/>
		/// </summary>
		/// <remarks>The constructor currently auto-initialises this to a just-in-time-bound <see cref="ConstructorTarget"/>
		/// targetting the <see cref="DecoratorType"/> by using the <see cref="ConstructorTarget.Auto(Type, IMemberBindingBehaviour)"/>
		/// method.</remarks>
		public ITarget Target { get; }
		/// <summary>
		/// Gets the target whose instance will be wrapped (decorated) by the one produced by 
		/// <see cref="Target"/>.
		/// </summary>
		public ITarget DecoratedTarget { get; }
		/// <summary>
		/// Gets the underlying type (e.g. a common service interface or base) that is being implemented
		/// through decoration.
		/// </summary>
		public Type DecoratedType { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="DecoratorTarget"/> type, initialising the <see cref="Target"/>
		/// to a just-in-time-bound <see cref="ConstructorTarget"/> for the <paramref name="decoratorType"/>.
		/// </summary>
		/// <param name="decoratorType">The type which is decorating the <paramref name="decoratedType"/></param>
		/// <param name="decoratedTarget">The target which is being decorated</param>
		/// <param name="decoratedType">The common type which is being decorated - e.g. <c>IService</c> when 
		/// the <paramref name="decoratedTarget"/> is bound to the type <c>MyService : IService</c> and
		/// the <paramref name="decoratorType"/> is set to <c>MyServiceDecorator : IService</c>.</param>
		public DecoratorTarget(Type decoratorType, ITarget decoratedTarget, Type decoratedType)
		{
			decoratorType.MustNotBeNull(nameof(decoratorType));
			decoratedTarget.MustNotBeNull(nameof(decoratedTarget));
			decoratedType.MustNotBeNull(nameof(decoratedType));

			if (!decoratedTarget.SupportsType(decoratedType))
				throw new ArgumentException("The decorated target doesn't support the decorated type", nameof(decoratedType));

			DecoratorType = decoratorType;
			DecoratedTarget = decoratedTarget;
			DecoratedType = decoratedType;
			//TODO: Allow a constructor to be supplied explicitly and potentially with parameter bindings
			Target = ConstructorTarget.Auto(DecoratorType);

			if (!Target.SupportsType(decoratedType))
				throw new ArgumentException("The decorator type is not compatible with the decorated type", nameof(decoratedType));

		}

		/// <summary>
		/// Overrides <see cref="TargetBase.SupportsType(Type)"/> to forward the call to <see cref="Target"/>.
		/// </summary>
		/// <param name="type">The type which is to be checked.</param>
		/// <returns><c>true</c> if the type is compatible with the object created by <see cref="Target"/>, <c>false</c>
		/// if not.</returns>
		public override bool SupportsType(Type type)
		{
			return Target.SupportsType(type);
		}
	}
}
