using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
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
	/// to work properly.
	/// </summary>
	/// <seealso cref="Rezolver.TargetBase" />
	public class DecoratorTarget : TargetBase
	{
		public override Type DeclaredType
		{
			get
			{
				return DecoratorType;
			}
		}

		/// <summary>
		/// Gets the type which is being used to decorate the instance produced by the 
		/// <see cref="DecoratedTarget"/> for the common service type <see cref="DecoratedType"/>
		/// </summary>
		/// <value>The type of the decorator.</value>
		public Type DecoratorType { get; }
		/// <summary>
		/// Gets the target which will create an instance of the <see cref="DecoratorType"/>
		/// </summary>
		/// <value>The target.</value>
		public ITarget Target { get; }
		/// <summary>
		/// Gets the target whose instance will be wrapped (decorated) by the one produced by 
		/// <see cref="Target"/>.
		/// </summary>
		/// <value>The decorated target.</value>
		public ITarget DecoratedTarget { get; }
		/// <summary>
		/// Gets the underlying type (e.g. a common service interface or base) that is being implemented
		/// through decoration..
		/// </summary>
		/// <value>The type of the decorated.</value>
		public Type DecoratedType { get; }

		public DecoratorTarget(Type decoratorType, ITarget decoratedTarget, Type decoratedType)
		{
			DecoratorType = decoratorType;
			DecoratedTarget = decoratedTarget;
			DecoratedType = decoratedType;
			//TODO: Allow a constructor to be supplied explicitly and potentially with parameter bindings
			Target = ConstructorTarget.Auto(DecoratorType);
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			//create a new context in order to create a new ChildBuilder into which we can register targets for look up
			var newContext = new CompileContext(context, inheritSharedExpressions: true);
			//add the decorated target into the compile context under the type which the enclosing decorator
			//was registered against.  If the inner target is bound to a type which correctly implements the decorator
			//pattern over the common decorated type, then the decorated instance should be resolved when constructor
			//arguments are resolved.
			newContext.Register(DecoratedTarget, DecoratedType);
			var expr = Target.CreateExpression(newContext);
			return expr;
#error need a decoratortargetbuilder now
		}

		public override bool SupportsType(Type type)
		{
			return Target.SupportsType(type);
		}
	}
}
