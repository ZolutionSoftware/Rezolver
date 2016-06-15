using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public class ConstructorTarget : TargetBase
	{
		/// <summary>
		/// A target that helps a constructor target bind to the best matching constructor on a type
		/// given a rezolver's configured services.
		/// 
		/// The target doesn't actually bind to a constructor until it's first called, and when it does,
		/// it does it based on the configured services of the rezolver that's in scope when the expression is
		/// first requested (so that's either creating an expression to give to another target, or to be compiled
		/// for its own compiled target).
		/// </summary>
		private class BestMatchConstructorTarget : TargetBase
		{
			private readonly IPropertyBindingBehaviour _propertyBindingBehaviour;

			private readonly Type _declaredType;
			public override Type DeclaredType
			{
				get { return _declaredType; }
			}

			private readonly object _locker = new object();
			private ITarget _wrapped = null;

			public BestMatchConstructorTarget(Type declaredType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
			{
				declaredType.MustNotBeNull(nameof(declaredType));
				_declaredType = declaredType;
				_propertyBindingBehaviour = propertyBindingBehaviour;
			}

			protected override Expression CreateExpressionBase(CompileContext context)
			{
				throw new NotImplementedException("This needs to be implemented");
#error implement this!
			}
		}


		private static readonly Type[] EmptyTypes = new Type[0];

		private readonly Type _declaredType;
		protected readonly ConstructorInfo _ctor;
		private readonly ParameterBinding[] _parameterBindings;

		public ConstructorTarget(Type type, ConstructorInfo ctor, params ParameterBinding[] parameterBindings)
		{
			_declaredType = type;
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

		public static ITarget Auto<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return Auto(typeof(T), propertyBindingBehaviour);
		}

		/// <summary>
		/// Creates a target that will create a new concrete instance of the <paramref name="type"/>.
		/// 
		/// Note - if the type is a generic type definition, t
		/// </summary>
		/// <param name="type"></param>
		/// <param name="propertyBindingBehaviour"></param>
		/// <returns></returns>
		public static ITarget Auto(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			//conduct a very simple search for the constructor with the most parameters
			type.MustNotBeNull(nameof(type));
			type.MustNot(t => TypeHelpers.IsInterface(t), "Type must not be an interface", nameof(type));
			type.MustNot(t => TypeHelpers.IsAbstract(t), "Type must not be abstract", nameof(type));

			if (TypeHelpers.IsGenericTypeDefinition(type))
				return new GenericConstructorTarget(type, propertyBindingBehaviour);

			IGrouping<int, ConstructorInfo>[] ctorGroups = GetPublicConstructorGroups(type);
			//get the first group - if there's more than one constructor then we can't choose.
			var ctorsWithMostParams = ctorGroups[0].ToArray();
			if (ctorsWithMostParams.Length > 1)
				throw new ArgumentException(
					string.Format(ExceptionResources.MoreThanOneConstructorFormat, type));

			var baseTarget = new ConstructorTarget(type, ctorsWithMostParams[0], ParameterBinding.BindWithRezolvedArguments(ctorsWithMostParams[0]));
			if (propertyBindingBehaviour != null)
				return new ConstructorWithPropertiesTarget(baseTarget, propertyBindingBehaviour.GetPropertyBindings(type));
			return baseTarget;
		}

		private static IGrouping<int, ConstructorInfo>[] GetPublicConstructorGroups(Type declaredType)
		{
			var ctorGroups = TypeHelpers.GetConstructors(declaredType)
							.GroupBy(c => c.GetParameters().Length)
							.OrderByDescending(g => g.Key).ToArray();

			if (ctorGroups.Length == 0)
				throw new ArgumentException(
					string.Format(ExceptionResources.NoPublicConstructorsDefinedFormat, declaredType), "declaredType");
			return ctorGroups;
		}

		/// <summary>
		/// Constructs an IRezolver
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="newExpr"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		public static ITarget For<T>(Expression<Func<RezolveContextExpressionHelper, T>> newExpr = null, ITargetAdapter adapter = null)
		{
			NewExpression newExprBody = null;
			if (newExpr != null)
			{
				newExprBody = newExpr.Body as NewExpression;
				if (newExprBody == null)
					throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyIsNotNewExpressionFormat, newExpr), "newExpr");
				else if (newExprBody.Type != typeof(T))
					throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyNewExpressionIsWrongTypeFormat, newExpr, typeof(T)), "newExpr");
			}

			return For(typeof(T), newExprBody, adapter);
		}

		public static ITarget Best<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return new BestMatchConstructorTarget(typeof(T), propertyBindingBehaviour);
		}

		public static ITarget WithArgs<T>(IDictionary<string, ITarget> args)
		{
			args.MustNotBeNull("args");

			return WithArgsInternal(typeof(T), args);
		}

		public static ITarget WithArgs(Type declaredType, IDictionary<string, ITarget> args)
		{
			declaredType.MustNotBeNull("declaredType");
			args.MustNotBeNull("args");

			return WithArgsInternal(declaredType, args);
		}

		public static ITarget WithArgs(Type declaredType, ConstructorInfo ctor, IDictionary<string, ITarget> args)
		{
			declaredType.MustNotBeNull("declaredType");
			ctor.MustNotBeNull("ctor");
			args.MustNotBeNull("args");

			ParameterBinding[] bindings = null;

			if (!ParameterBinding.BindMethod(ctor, args, out bindings))
				throw new ArgumentException("Cannot bind constructor with supplied arguments", "args");

			return new ConstructorTarget(declaredType, ctor, bindings);
		}

		internal static ITarget WithArgsInternal(Type declaredType, IDictionary<string, ITarget> args)
		{
			MethodBase ctor = null;
			var bindings = ParameterBinding.BindOverload(TypeHelpers.GetConstructors(declaredType), args, out ctor);

			return new ConstructorTarget(declaredType, (ConstructorInfo)ctor, bindings);
		}

		internal static ITarget For(Type declaredType, NewExpression newExpr = null, ITargetAdapter adapter = null)
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
								ExceptionResources.NoDefaultOrAllOptionalConstructorFormat,
								declaredType), "declaredType");
				}
			}
			else
			{
				ctor = newExpr.Constructor;
				parameterBindings = ExtractParameterBindings(newExpr, adapter ?? TargetAdapter.Default).ToArray();

			}

			if (parameterBindings == null)
				parameterBindings = ParameterBinding.DeriveDefaultParameterBindings(ctor);
			return new ConstructorTarget(declaredType, ctor, parameterBindings);
		}

		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, ITargetAdapter adapter)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.CreateTarget(expression))).ToArray();
		}
	}
}