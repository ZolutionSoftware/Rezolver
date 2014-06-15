using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public class ConstructorTarget : RezolveTargetBase
	{
		private readonly Type[] EmptyTypes = new Type[0];

		private readonly Type _declaredType;
		private readonly ConstructorInfo _ctor;

		public ConstructorTarget(Type declaredType)
		{
			declaredType.MustNotBeNull("declaredType");
			_declaredType = declaredType;
			//conduct search for default constructor straight away.  Chuck an exception if not found.
			_ctor = declaredType.GetConstructor(EmptyTypes);
			if(_ctor == null)
				throw new ArgumentException(string.Format("The type {0} has no default constructor", declaredType),  "declaredType");
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			return Expression.Convert(Expression.New(_ctor), targetType ?? DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}
}