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
		private readonly PropertyOrFieldBinding[] _propertyBindings;

		public ConstructorTarget(Type declaredType, ConstructorInfo ctor, params ParameterBinding[] parameterBindings)
		{
			_declaredType = declaredType;
			_ctor = ctor;
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
			_propertyBindings = PropertyOrFieldBinding.None;
		}

		public ConstructorTarget(Type declaredType, ConstructorInfo ctor, PropertyOrFieldBinding[] propertyBindings, ParameterBinding[] parameterBindings)
		{
			_declaredType = declaredType;
			_ctor = ctor;
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
			_propertyBindings = propertyBindings ?? PropertyOrFieldBinding.None;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			if(_propertyBindings.Length == 0)
				return CreateNewExpression(context);
			else
				return Expression.MemberInit(CreateNewExpression(context), 
					_propertyBindings.Select(pb => Expression.Bind(pb.Member, pb.Target.CreateExpression(new CompileContext(context, pb.MemberType, true)))));
		}

		private NewExpression CreateNewExpression(CompileContext context)
		{
			return Expression.New(_ctor,
								_parameterBindings.Select(pb => pb.Target.CreateExpression(new CompileContext(context, pb.Parameter.ParameterType, true))));
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

		public static ConstructorTarget For<T>(Expression<Func<RezolveContextExpressionHelper, T>> newExpr = null, IRezolveTargetAdapter adapter = null)
		{
			NewExpression newExprBody = null;
			if (newExpr != null)
			{
				newExprBody = newExpr.Body as NewExpression;
				if (newExprBody == null)
					throw new ArgumentException(string.Format(Exceptions.LambdaBodyIsNotNewExpressionFormat, newExpr), "newExpr");
				else if (newExprBody.Type != typeof(T))
					throw new ArgumentException(string.Format(Exceptions.LambdaBodyNewExpressionIsWrongTypeFormat, newExpr, typeof(T)), "newExpr");
			}

			return For(typeof(T), newExprBody, adapter);
		}

		internal static ConstructorTarget For(Type declaredType, NewExpression newExpr = null, IRezolveTargetAdapter adapter = null)
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
				parameterBindings = ExtractParameterBindings(newExpr, adapter ?? RezolveTargetAdapter.Default).ToArray();

			}

			if (parameterBindings == null)
				parameterBindings = ParameterBinding.DeriveDefaultParameterBindings(ctor);
			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}

		//internal static ConstructorTarget For(Type declaredType, MemberInitExpression initExpr, IRezolveTargetAdapter adapter = null)
		//{
		//	ParameterBinding[] parameterBindings = ExtractParameterBindings(initExpr.NewExpression, adapter ?? RezolveTargetAdapter.Default).ToArray();
		//	PropertyOrFieldBinding[] propertyBindings = initExpr.Bindings.Select(mb => new PropertyOrFieldBinding(mb.Member, adapter.GetRezolveTarget(mb))).ToArray();
		//}

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, IRezolveTargetAdapter adapter)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.GetRezolveTarget(expression))).ToArray();
		}
	}
}