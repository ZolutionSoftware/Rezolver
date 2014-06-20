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
				parameterBindings = ExtractParameterBindings(newExpr).ToArray();

			}

			if(parameterBindings == null)
				parameterBindings = DeriveParameterBindings(ctor);
			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) =>
				{
					RezolverScopeExtensions.RezolveCallExpressionInfo rezolveCallArg =
						RezolverScopeExtensions.ExtractRezolveCall(expression);

					return new ParameterBinding(info,
						rezolveCallArg != null
							? (IRezolveTarget)new RezolvedTarget(rezolveCallArg)
							: new ExpressionTarget(expression));
				}).ToArray();
		}
	}

	/// <summary>
	/// Represents a target that is rezolved during expression building 
	/// </summary>
	internal class RezolvedTarget : RezolveTargetBase
	{
		private readonly RezolverScopeExtensions.RezolveCallExpressionInfo _rezolveCall;

		public RezolvedTarget(RezolverScopeExtensions.RezolveCallExpressionInfo rezolveCall)
		{
			_rezolveCall = rezolveCall;
		}

		public override Type DeclaredType
		{
			get { return _rezolveCall.Type; }
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			scope.MustNotBeNull("scope");
			//basic - without supporting a name
			var resolvedTarget = scope.Fetch(_rezolveCall.Type, null);
			if(resolvedTarget == null)
				throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _rezolveCall.Type));
			return Expression.Convert(resolvedTarget.CreateExpression(scope), targetType ?? _rezolveCall.Type);
		}
	}
}