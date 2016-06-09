using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	public class DecoratorTarget : ITargetContainer
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

			public DecoratingTarget(Type decoratorType, ITarget decorated, Type type)
			{
				_decoratorType = decoratorType;
				_decorated = decorated;
				_type = type;
			}

			protected override Expression CreateExpressionBase(CompileContext context)
			{
				var newContext = new CompileContext(context, inheritSharedExpressions: true);
				newContext.Register(_decorated, _type);
				var ctorTarget = ConstructorTarget.Auto(_decoratorType);
				var expr = ctorTarget.CreateExpression(newContext);
				return expr;
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
			if (_inner == null) _inner = new TargetListContainer(serviceType, target);
			else
				_inner.Register(target, serviceType);
		}
	}
}
