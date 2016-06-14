using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Implements decoration in an <see cref="ITargetContainer"/>
	/// </summary>
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
				var newContext = new CompileContext(context, inheritSharedExpressions: true);
				newContext.Register(_decorated, _type);
				var expr = _innerTarget.CreateExpression(newContext);
				return expr;
			}

			public override bool SupportsType(Type type)
			{
				return _innerTarget.SupportsType(type);
			}
		}

		private readonly Type _decoratorType;
		private readonly Type _decoratedType;
		private ITargetContainer _inner;

		public DecoratorTarget(Type decoratorType, Type decoratedType)
		{
			_decoratorType = decoratorType;
			_decoratedType = decoratedType;
		}

		private void EnsureInnerContainer()
		{
			if (_inner != null) return;
			//similar logic to the Builder class here - if the type we're decorating is a generic 
			//then we will use a GenericTargetContainer.  Otherwise, we'll use a TargetListContainer
			if (TypeHelpers.IsGenericType(_decoratedType))
			{
				_inner = new GenericTargetContainer(
					TypeHelpers.IsGenericTypeDefinition(_decoratedType) ?
						_decoratedType
						: _decoratedType.GetGenericTypeDefinition());
			}
			else
				_inner = new TargetListContainer(_decoratedType);
		}

		public ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			if (_inner != null)
				throw new InvalidOperationException("Already decorating another container");

			_inner = existing;
			return this;
		}

		public ITarget Fetch(Type type)
		{
			if (_inner == null)
				return null;

			var result = _inner.Fetch(type);
			if (result != null)
				return new DecoratingTarget(_decoratorType, result, type);
			return null;
		}

		public IEnumerable<ITarget> FetchAll(Type type)
		{
			if (_inner == null)
				return Enumerable.Empty<ITarget>();
			return _inner.FetchAll(type).Select(t => new DecoratingTarget(_decoratorType, t, type));
		}

		public void Register(ITarget target, Type serviceType = null)
		{
			EnsureInnerContainer();

			_inner.Register(target, serviceType);
		}

		public ITargetContainer FetchContainer(Type type)
		{
			EnsureInnerContainer();
			if (_inner is ITargetContainerOwner)
				return ((ITargetContainerOwner)_inner).FetchContainer(type);
			throw new InvalidOperationException("This decorator must be decorating another owner, or be decorating a generic type");
		}

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
