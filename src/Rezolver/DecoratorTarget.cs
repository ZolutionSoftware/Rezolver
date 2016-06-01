using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	public class DecoratorTarget : RezolveTargetBase
	{
		public virtual Type DecoratedType { get; }
		public override Type DeclaredType { get; }

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			throw new NotImplementedException();
		}

		public DecoratorTarget(Type decoratedType, Type declaredType)
		{
			DecoratedType = decoratedType;
			DeclaredType = declaredType;
		}
	}
}
