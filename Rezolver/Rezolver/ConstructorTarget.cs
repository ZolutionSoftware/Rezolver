using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Resources;

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

		public static ConstructorTarget For<T>(Expression<Func<IRezolverScope, T>> newExpr = null)
		{
			NewExpression newExprBody = null;
			if (newExpr != null)
			{
				newExprBody = newExpr.Body as NewExpression;
				if(newExprBody == null)
					throw new ArgumentException(string.Format(Exceptions.LambdBodyIsNotNewExpressionFormat, newExpr), "newExpr");
			}

			return For(typeof(T), newExprBody);
		}

		internal static ConstructorTarget For(Type declaredType, NewExpression newExpr = null)
		{
			ConstructorInfo ctor = null;
			ParameterBinding[] parameterBindings = null;

			if (newExpr == null)
			{
				ctor = declaredType.GetConstructor(EmptyTypes);

				if (ctor == null)
				{
					ctor = declaredType.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
					if (ctor == null)
						throw new ArgumentException(
							string.Format(
								Exceptions.NoDefaultOrAllOptionalConstructorFormat,
								declaredType), "declaredType");
					
				}
			}
			else
			{
				ctor = newExpr.Constructor;
				parameterBindings = ConvertToParameterBindings(ctor.GetParameters(), newExpr.Arguments);
			}

			if(parameterBindings == null)
				parameterBindings = DeriveParameterBindings(ctor);
			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}
	}
}