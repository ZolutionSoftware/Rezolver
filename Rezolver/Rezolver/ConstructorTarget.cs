﻿using System;
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

		public ConstructorTarget(Type declaredType, ConstructorInfo ctor, params ParameterBinding[] parameterBindings)
		{
			_declaredType = declaredType;
			_ctor = ctor;
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			return Expression.Convert(Expression.New(_ctor,
				_parameterBindings.Select(pb => pb.Target.CreateExpression(scope))), targetType ?? DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}

		public static ConstructorTarget Auto<T>()
		{
			return Auto(typeof(T));
		}

		public static ConstructorTarget Auto(Type declaredType)
		{
			//conduct a very simple search for the constructor with the most parameters
			declaredType.MustNotBeNull("declaredType");

			var ctorGroups = declaredType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.GroupBy(c => c.GetParameters().Length)
				.OrderByDescending(g => g.Key).ToArray();

			if (ctorGroups.Length == 0)
				throw new ArgumentException(
					string.Format(Exceptions.NoPublicConstructorsDefinedFormat, declaredType), "declaredType");
			//get the first group - if there's more than one constructor then we can't choose.
			var ctorsWithMostParams = ctorGroups[0].ToArray();
			if (ctorsWithMostParams.Length > 1)
				throw new ArgumentException(
					string.Format(Exceptions.MoreThanOneConstructorFormat, declaredType));
			return new ConstructorTarget(declaredType, ctorsWithMostParams[0], ParameterBinding.DeriveAutoParameterBindings(ctorsWithMostParams[0]));
		}

		public static ConstructorTarget For<T>(Expression<Func<IRezolverScope, T>> newExpr = null)
		{
			NewExpression newExprBody = null;
			if (newExpr != null)
			{
				newExprBody = newExpr.Body as NewExpression;
				if (newExprBody == null)
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

			if (parameterBindings == null)
				parameterBindings = ParameterBinding.DeriveDefaultParameterBindings(ctor);
			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) =>
				{
					//RezolverScopeExtensions.RezolveCallExpressionInfo rezolveCallArg =
					//	RezolverScopeExtensions.ExtractRezolveCall(expression);
					//return new ParameterBinding(info,
						
					//	rezolveCallArg != null
					//		? (IRezolveTarget)new RezolvedTarget(rezolveCallArg)
					//		: new ExpressionTarget(expression));
					//TODO: get this bit working - Need a general purpose expression adapter, whose responsibility it is to turn expressions into targets.
					return new ParameterBinding(info, expression.AsRezolveTarget());
				}).ToArray();
		}
	}
}