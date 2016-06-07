using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Represents a decorator for other services.
	/// </summary>
	/// <remarks>Implementation note: the class implements both IRezolveTarget and IRezolveTargetEntry, which is
	/// detected by the RezolverBuilder-deriving classes so that the internal registrations of targets against types
	/// can be manipulated correctly to support these types of scenarios.</remarks>
	public class DecoratorTarget : TargetBase, IRezolveTargetEntry
	{
		private class DecoratorTargetProxyEntry : IRezolveTargetEntry
		{
			
			private readonly IRezolveTargetEntry _entry;
			public DecoratorTargetProxyEntry(IRezolveTargetEntry entry)
			{
				_entry = entry;
			}

			public Type DeclaredType
			{
				get
				{
					return _entry.DeclaredType;
				}
			}

			public ITarget DefaultTarget
			{
				get
				{
					return _entry.DefaultTarget;
				}
			}

			public ITargetContainer ParentBuilder
			{
				get
				{
					return _entry.ParentBuilder;
				}
			}

			public Type RegisteredType
			{
				get
				{
					return _entry.RegisteredType;
				}
			}

			public IEnumerable<ITarget> Targets
			{
				get
				{
					return _entry.Targets;
				}
			}

			public bool UseFallback
			{
				get
				{
					return _entry.UseFallback;
				}
			}

			public void AddTarget(ITarget target, bool checkForDuplicates = false)
			{
				_entry.AddTarget(target, checkForDuplicates);
			}

			public void Attach(ITargetContainer parentBuilder, IRezolveTargetEntry replacing = null)
			{
				//only thing that doesn't get forwarded.  Why?  because of the way we
				//implement decoration during compilation - i.e. with additional registrations
				//in a transient rezolver builder belonging to the compile context.
			}

			public Expression CreateExpression(CompileContext context)
			{
				return _entry.CreateExpression(context);
			}

			public bool SupportsType(Type type)
			{
				return _entry.SupportsType(type);
			}
		}

		private bool _isAttached;
		/// <summary>
		/// The type that is decorated by this target.
		/// 
		/// Note - this is always the type that this target is registered for in a builder.
		/// </summary>
		public virtual Type DecoratedType { get; }
		/// <summary>
		/// The type that will be created, decorating the <see cref="DecoratedType"/>
		/// </summary>
		public override Type DeclaredType { get; }

		public ITargetContainer ParentBuilder
		{
			get; private set;
		}

		public Type RegisteredType
		{
			get
			{
				return DecoratedType;
			}
		}

		public ITarget DefaultTarget
		{
			get
			{
				if (_decorated == null) return null;
				return new DecoratorTarget(this, _decorated.DefaultTarget);
			}
		}

		/// <summary>
		/// Implements the property by returning transient copies of this decorator with individual 
		/// targets from the decorated entry's <see cref="IRezolveTargetEntry.Targets"/> enumerable.
		/// </summary>
		public IEnumerable<ITarget> Targets
		{
			get
			{
				if (_decorated == null) yield break;
				foreach(var target in _decorated.Targets)
				{
					yield return new DecoratorTarget(this, target);
				}
			}
		}

		public bool UseFallback
		{
			get
			{
				return false;
			}
		}
		private void ThrowIfInvalid()
		{
			if (ParentBuilder == null)
				throw new InvalidOperationException("This target cannot be used until Attach has been called");
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			ThrowIfInvalid();
			//TODO: This does not honour IEnumerables yet
			var newContext = new CompileContext(context, inheritSharedExpressions: true);
			//have to proxy the decorated entry so that the Attach operation that the registration
			//could trigger will not throw an exception.
			newContext.Register(new DecoratorTargetProxyEntry(_decorated), RegisteredType);
			var ctorTarget = ConstructorTarget.Auto(DeclaredType);
			var expr = ctorTarget.CreateExpression(newContext);
			return expr;

		}

		public void AddTarget(ITarget target, bool checkForDuplicates = false)
		{
			ThrowIfInvalid();
			//TODO: Consider adding the CreateEntry metehod back into the RezolverBuilder class and 
			//then extracting it to the IRezolveTargetContainer interface so it can be called here.
			if (_decorated == null)
				_decorated = new RezolveTargetEntry(ParentBuilder, RegisteredType, target);
			else
			{
				//note that if we decorate another decorator, this will eventually channel through to the actual wrapped entry
				_decorated.AddTarget(target, checkForDuplicates);
			}
		}

		private IRezolveTargetEntry _decorated;

		public void Attach(ITargetContainer parentBuilder, IRezolveTargetEntry existing)
		{
			parentBuilder.MustNotBeNull(nameof(parentBuilder));
			//allow multiple attaches to the same builder.
			if (ParentBuilder != null && parentBuilder != ParentBuilder)
				throw new InvalidOperationException("This method has already been called.");

			if (existing != null && existing.RegisteredType != DecoratedType)
				throw new ArgumentException($"The registered type of the entry that is being replaced must equal this entry's RegisteredType ({ RegisteredType })", nameof(existing));

			if (_decorated != null && existing != _decorated)
				throw new InvalidOperationException("You cannot call this method twice for the same builder with a different target being replaced.");

			ParentBuilder = parentBuilder;
			_decorated = existing;
		}

		
		public DecoratorTarget(Type decoratedType, Type decoratorType)
		{
			decoratedType.MustNotBeNull(nameof(decoratedType));
			DecoratedType = decoratedType;
			DeclaredType = decoratorType;
		}

		/// <summary>
		/// Private only constructor, used to construct a new decorator target from this target, wrapping a single target
		/// </summary>
		/// <param name="parentBuilder"></param>
		/// <param name="decorated"></param>
		/// <param name="decoratorType"></param>
		private DecoratorTarget(DecoratorTarget source, ITarget singleTarget)
		{
			DecoratedType = source.DecoratedType;
			DeclaredType = source.DeclaredType;
			Attach(source.ParentBuilder, new RezolveTargetEntry(source.ParentBuilder, source.RegisteredType, singleTarget));
		}
	}
}
