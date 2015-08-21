using System;
using System.Collections.Generic;
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

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			return CreateNewExpression(context);
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

		public static IRezolveTarget Auto<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return Auto(typeof(T), propertyBindingBehaviour);
		}

		public static IRezolveTarget Auto(Type declaredType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			//conduct a very simple search for the constructor with the most parameters
			declaredType.MustNotBeNull("declaredType");

			var ctorGroups = TypeHelpers.GetConstructors(declaredType)
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

			var baseTarget = new ConstructorTarget(declaredType, ctorsWithMostParams[0], ParameterBinding.DeriveAutoParameterBindings(ctorsWithMostParams[0]));
			if (propertyBindingBehaviour != null)
				return new ConstructorWithPropertiesTarget(baseTarget, propertyBindingBehaviour.GetPropertyBindings(declaredType));
			return baseTarget;
		}

		public static IRezolveTarget For<T>(Expression<Func<RezolveContextExpressionHelper, T>> newExpr = null, IRezolveTargetAdapter adapter = null)
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

		public static IRezolveTarget WithArgs<T>(IDictionary<string, IRezolveTarget> args)
		{
			args.MustNotBeNull("args");

			return WithArgsInternal(typeof(T), args);
		}

		public static IRezolveTarget WithArgs(Type declaredType, IDictionary<string, IRezolveTarget> args)
		{
			declaredType.MustNotBeNull("declaredType");
			args.MustNotBeNull("args");

			return WithArgsInternal(declaredType, args);
		}

		public static IRezolveTarget WithArgs(Type declaredType, ConstructorInfo ctor, IDictionary<string, IRezolveTarget> args)
		{
			declaredType.MustNotBeNull("declaredType");
			ctor.MustNotBeNull("ctor");
			args.MustNotBeNull("args");

			ParameterBinding[] bindings = null;

			if (!ParameterBinding.BindMethod(ctor, args, out bindings))
				throw new ArgumentException("Cannot bind constructor with supplied arguments", "args");

			return new ConstructorTarget(declaredType, ctor, bindings);
		}

		internal static IRezolveTarget WithArgsInternal(Type declaredType, IDictionary<string, IRezolveTarget> args)
		{
			MethodBase ctor = null;
			var bindings = ParameterBinding.BindOverload(TypeHelpers.GetConstructors(declaredType), args, out ctor);

			return new ConstructorTarget(declaredType, (ConstructorInfo)ctor, bindings);
		}

		internal static IRezolveTarget For(Type declaredType, NewExpression newExpr = null, IRezolveTargetAdapter adapter = null)
		{
			ConstructorInfo ctor = null;
			ParameterBinding[] parameterBindings = null;

			if (newExpr == null)
			{
				ctor = TypeHelpers.GetConstructor(declaredType, EmptyTypes);

				if (ctor == null)
				{
					ctor = TypeHelpers.GetConstructors(declaredType).FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
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

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, IRezolveTargetAdapter adapter)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.GetRezolveTarget(expression))).ToArray();
		}
	}
}