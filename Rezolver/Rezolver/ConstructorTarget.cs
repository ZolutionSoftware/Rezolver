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
		private readonly ParameterBinding[] _parameterBindings;

		private ConstructorTarget(Type declaredType, ConstructorInfo ctor, params ParameterBinding[] parameterBindings)
		{
			_declaredType = declaredType;
			_ctor = ctor;
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			if (_parameterBindings.Length == 0)
				return Expression.Convert(Expression.New(_ctor), targetType ?? DeclaredType);
			else
				return Expression.Convert(
					Expression.New(_ctor, _parameterBindings.Select(pb => pb.Target.CreateExpression(scope))),
					targetType ?? DeclaredType);

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
			ParameterBinding[] parameterBindings = null;
			if (ctor == null)
			{
				ctor = declaredType.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
				if(ctor == null)
					throw new ArgumentException(string.Format("The type {0} has no default constructor, nor any constructors where all the parameters are optional.", declaredType), "declaredType");
				parameterBindings = DeriveParameterBindings(ctor);
			}

			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}
	}
}