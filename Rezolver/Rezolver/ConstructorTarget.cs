using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public class ConstructorTarget : RezolveTargetBase
	{
		private static readonly Type[] EmptyTypes = new Type[0];

		private readonly Type _declaredType;
		protected readonly ConstructorInfo _ctor;

		private ConstructorTarget(Type declaredType, ConstructorInfo ctor)
		{
			_declaredType = declaredType;
			_ctor = ctor;
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			return Expression.Convert(Expression.New(_ctor), targetType ?? DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}

		public static ConstructorTarget For<T>()
		{
			return For(typeof (T));
		}

		internal static ConstructorTarget For(Type declaredType)
		{
			var ctor = declaredType.GetConstructor(EmptyTypes);
			if (ctor == null)
			{
				ctor = declaredType.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
				if(ctor == null)
					throw new ArgumentException(string.Format("The type {0} has no default constructor, nor any constructors where all the parameters are optional.", declaredType), "declaredType");
			}
			//TODO: parameter bindings will need to be added to this.
			return new ConstructorTarget(declaredType, ctor);
		}
	}

	//public class ConstructorTarget<T> : ConstructorTarget
	//{
	//	public ConstructorTarget(Expression<Func<T>> newExpr)
	//	{
	//		newExpr.MustNotBeNull("newExpr");
	//	}
	//}
}