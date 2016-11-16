// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Implements service decoration in an <see cref="ITargetContainer"/>.
	/// 
	/// The best way to add a decorator to your target container is to use the extension method <see cref="ITargetContainerExtensions.RegisterDecorator{TDecorator, TDecorated}(ITargetContainerOwner)"/>
	/// or its non-generic equivalent.
	/// </summary>
	/// <remarks>This class does not implement <see cref="ITarget"/>, rather
	/// it's an <see cref="ITargetContainerOwner"/> into which other targets can be added,
	/// and when <see cref="Fetch(Type)"/> or <see cref="FetchAll(Type)"/> are called, a temporary
	/// target object is created which wraps around the targets that have been registered within and
	/// which will ultimately create instances of <see cref="DecoratorType"/></remarks>
	public class DecoratorTarget : ITargetContainerOwner
	{
		private class DecoratingTarget : TargetBase
		{
			public override Type DeclaredType
			{
				get
				{
					return _decoratorType;
				}
			}

			private Type _decoratorType;
			private ITarget _decorated;
			private Type _type;
			private ITarget _innerTarget;

			public DecoratingTarget(Type decoratorType, ITarget decorated, Type type)
			{
				_decoratorType = decoratorType;
				_decorated = decorated;
				_type = type;
				//TODO: Allow a constructor to be supplied explicitly and potentially with parameter bindings
				_innerTarget = ConstructorTarget.Auto(_decoratorType);
			}

			protected override Expression CreateExpressionBase(CompileContext context)
			{
				//create a new context in order to create a new ChildBuilder into which we can register targets for look up
				var newContext = new CompileContext(context, inheritSharedExpressions: true);
				//add the decorated target into the compile context under the type which the enclosing decorator
				//was registered against.  If the inner target is bound to a type which correctly implements the decorator
				//pattern over the common decorated type, then the decorated instance should be resolved when constructor
				//arguments are resolved.
				newContext.Register(_decorated, _type);
				var expr = _innerTarget.CreateExpression(newContext);
				return expr;
			}

			public override bool SupportsType(Type type)
			{
				return _innerTarget.SupportsType(type);
			}
		}

		/// <summary>
		/// Gets the type which will be used to decorate the instances produced by targets in this decorator target.
		/// </summary>
		public Type DecoratorType { get; }

		/// <summary>
		/// Gets the type that's being decorated - in essence, this is the type that this decorator target
		/// </summary>
		public Type DecoratedType { get; }

		private ITargetContainer _inner;

		/// <summary>
		/// Initializes a new instance of the <see cref="DecoratorTarget"/> class.
		/// </summary>
		/// <param name="decoratorType">Type of the decorator.</param>
		/// <param name="decoratedType">Type being decorated.</param>
		public DecoratorTarget(Type decoratorType, Type decoratedType)
		{
			DecoratorType = decoratorType;
			DecoratedType = decoratedType;
		}

		private void EnsureInnerContainer()
		{
			if (_inner != null) return;
			//similar logic to the Builder class here - if the type we're decorating is a generic 
			//then we will use a GenericTargetContainer.  Otherwise, we'll use a TargetListContainer
			if (TypeHelpers.IsGenericType(DecoratedType))
			{
				_inner = new GenericTargetContainer(
				  TypeHelpers.IsGenericTypeDefinition(DecoratedType) ?
					DecoratedType
					: DecoratedType.GetGenericTypeDefinition());
			}
			else
				_inner = new TargetListContainer(DecoratedType);
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> by wrapping the 
		/// <paramref name="existing"/> container and returning itself.
		/// 
		/// This allows decorators to be applied on top of decorators; and decorators to be added after types
		/// have begun to be registered in another target container.
		/// </summary>
		/// <param name="existing">The existing <see cref="ITargetContainer" /> instance that this instance is to be combined with</param>
		/// <param name="type">The type that the combined container owner will be registered under.</param>
		/// <exception cref="InvalidOperationException">If this target container is already decorating another container</exception>
		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			if (_inner != null)
				throw new InvalidOperationException("Already decorating another container");

			_inner = existing;
			return this;
		}

		/// <summary>
		/// Implementation of <see cref="ITargetContainer.Fetch(Type)"/> - wraps a special target around
		/// the target returned by the target container that's decorated by this one.
		/// </summary>
		/// <param name="type">Required.  The type for which an <see cref="ITarget" /> is to be retrieved.</param>
		/// <remarks>If the inner container returns null, then so does this one.</remarks>
		public ITarget Fetch(Type type)
		{
			if (_inner == null)
				return null;

			var result = _inner.Fetch(type);
			if (result != null)
				return new DecoratingTarget(DecoratorType, result, type);
			return null;
		}

		/// <summary>
		/// Implementation of <see cref="ITargetContainer.FetchAll(Type)"/> - passes the call on to the inner 
		/// container that's decorated by this one, and then wraps each of those targets in a special target which
		/// represents the decoration logic for each instance.
		/// </summary>
		/// <param name="type">Required.  The type for which the <see cref="ITarget" /> instances are to be retrieved.</param>
		public IEnumerable<ITarget> FetchAll(Type type)
		{
			if (_inner == null)
				return Enumerable.Empty<ITarget>();
			return _inner.FetchAll(type).Select(t => new DecoratingTarget(DecoratorType, t, type));
		}

		/// <summary>
		/// Registers a target, either for the <paramref name="serviceType" /> specified or, if null, the <see cref="ITarget.DeclaredType" />
		/// of the <paramref name="target" />.
		/// </summary>
		/// <param name="target">Required.  The target to be registered</param>
		/// <param name="serviceType">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="ITarget.DeclaredType" /> of the <paramref name="target" />.  If provided, then the <paramref name="target" />
		/// must be compatible with this type.</param>
		/// <remarks>The decorator target does not accept registrations directly; rather it passes the call on to its
		/// inner container which could be a <see cref="TargetListContainer"/>, or <see cref="GenericTargetContainer"/> in 
		/// the most basic cases; or it could be another <see cref="DecoratorTarget"/> in situations where a type has had
		/// multiple decorators registered against it.</remarks>
		public void Register(ITarget target, Type serviceType = null)
		{
			EnsureInnerContainer();

			_inner.Register(target, serviceType);
		}

		/// <summary>
		/// Retrieves an existing container registered against the given <paramref name="type" />, or null if not found.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <exception cref="InvalidOperationException">If this decorator's inner container isn't an instance of <see cref="ITargetContainerOwner"/></exception>
		/// <remarks>This is an implementation of <see cref="ITargetContainerOwner.FetchContainer(Type)"/> which wraps
		/// around the inner target container and passes the call on to that.</remarks>
		public ITargetContainer FetchContainer(Type type)
		{
			EnsureInnerContainer();
			if (_inner is ITargetContainerOwner)
				return ((ITargetContainerOwner)_inner).FetchContainer(type);
			throw new InvalidOperationException("This decorator must be decorating another owner, or be decorating a generic type");
		}

		/// <summary>
		/// Implementation of <see cref="ITargetContainerOwner.RegisterContainer(Type, ITargetContainer)"/> - the call is
		/// automatically forwarded on to the inner target container that's being decorated, since decorator targets don't support
		/// direct registration of targets or containers.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="container">The container.</param>
		/// <exception cref="InvalidOperationException">This decorator must be decorating another owner, or be decorating a generic type</exception>
		public void RegisterContainer(Type type, ITargetContainer container)
		{
			EnsureInnerContainer();
			if (_inner is ITargetContainerOwner)
				((ITargetContainerOwner)_inner).RegisterContainer(type, container);
			else
				throw new InvalidOperationException("This decorator must be decorating another owner, or be decorating a generic type");
		}
	}
}
