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
		/// <summary>
		/// A target that helps a constructor target bind to the best matching constructor on a type
		/// given a rezolver's configured services.
		/// 
		/// The target doesn't actually bind to a constructor until it's first called, and when it does,
		/// it does it based on the configured services of the rezolver that's in scope when the expression is
		/// first requested (so that's either creating an expression to give to another target, or to be compiled
		/// for its own compiled target).
		/// </summary>
		private class BestMatchConstructorTarget : RezolveTargetBase
		{
			private readonly IPropertyBindingBehaviour _propertyBindingBehaviour;

			private readonly Type _declaredType;
			public override Type DeclaredType
			{
				get { return _declaredType; }
			}

			private readonly object _locker = new object();
			private IRezolveTarget _wrapped = null;

			public BestMatchConstructorTarget(Type declaredType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
			{
				declaredType.MustNotBeNull(nameof(declaredType));
				_declaredType = declaredType;
				_propertyBindingBehaviour = propertyBindingBehaviour;
			}

			protected override Expression CreateExpressionBase(CompileContext context)
			{
				//only resolve the constructor once.
				if (_wrapped == null)
				{
					lock (_locker)
					{
						if (_wrapped == null)
						{

						}
						//_wrapped = ConstructorTarget.Auto(context.DependencyBuilder, _declaredType, _propertyBindingBehaviour, context.);
					}
				}
				return _wrapped.CreateExpression(context);
			}
		}


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

		/// <summary>
		/// Creates a target that will create a new concrete
		/// </summary>
		/// <param name="declaredType"></param>
		/// <param name="propertyBindingBehaviour"></param>
		/// <returns></returns>
		public static IRezolveTarget Auto(Type declaredType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			//conduct a very simple search for the constructor with the most parameters
			declaredType.MustNotBeNull("declaredType");

			if (TypeHelpers.IsGenericTypeDefinition(declaredType))
				return new GenericConstructorTarget(declaredType, propertyBindingBehaviour);

			IGrouping<int, ConstructorInfo>[] ctorGroups = GetPublicConstructorGroups(declaredType);
			//get the first group - if there's more than one constructor then we can't choose.
			var ctorsWithMostParams = ctorGroups[0].ToArray();
			if (ctorsWithMostParams.Length > 1)
				throw new ArgumentException(
					string.Format(Exceptions.MoreThanOneConstructorFormat, declaredType));

			var baseTarget = new ConstructorTarget(declaredType, ctorsWithMostParams[0], ParameterBinding.BindWithRezolvedArguments(ctorsWithMostParams[0]));
			if (propertyBindingBehaviour != null)
				return new ConstructorWithPropertiesTarget(baseTarget, propertyBindingBehaviour.GetPropertyBindings(declaredType));
			return baseTarget;
		}

		private static IGrouping<int, ConstructorInfo>[] GetPublicConstructorGroups(Type declaredType)
		{
			var ctorGroups = TypeHelpers.GetConstructors(declaredType)
							.GroupBy(c => c.GetParameters().Length)
							.OrderByDescending(g => g.Key).ToArray();

			if (ctorGroups.Length == 0)
				throw new ArgumentException(
					string.Format(Exceptions.NoPublicConstructorsDefinedFormat, declaredType), "declaredType");
			return ctorGroups;
		}

		/// <summary>
		/// This overload restricts the target to binding the best constructor whose arguments can actually be resolved
		/// from the <see cref="IRezolverBuilder"/> that you pass as the argument.
		/// 
		/// This overrides the default behaviour, which is to select the constructor with the most arguments.
		/// 
		/// Note - if the target that is created is to be registered in the builder with a path, then you must pass that path in the <paramref name="targetName"/>
		/// argument, otherwise you will get inconsistent results.
		/// </summary>
		/// <param name="dependencyLookup">The builder that will be used to look for </param>
		/// <param name="declaredType"></param>
		/// <param name="propertyBindingBehaviour"></param>
		/// <returns></returns>
		public static IRezolveTarget Auto(IRezolverBuilder dependencyLookup, Type declaredType, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			dependencyLookup.MustNotBeNull("dependencyLookup");
			declaredType.MustNotBeNull("declaredType");

			var ctorGroups = GetPublicConstructorGroups(declaredType);

			//search all ctor groups, attempting to match each parameter to a rezolve target
			//this is slightly cut down version of what's done by the RezolvedTarget, and not quite as clever:
			//it only considers the current IRezolverBuilder.
			var firstGroupWithMatch = ctorGroups.Select(g => new
			{
				paramCount = g.Key,
				matches = g.Select(c =>
				{
					var parameters = c.GetParameters();
					return new
					{
						constructor = c,
						bindings = parameters.Select(p => new
						{
							parameter = p,
							binding = dependencyLookup.Fetch(p.ParameterType)
						}).ToArray()
					};
				})
			}).FirstOrDefault(g => g.matches.Any(m => m.bindings.Length == 0 ? true : m.bindings.All(b => b.binding != null)));

			var matchesArray = firstGroupWithMatch.matches.ToArray();

			throw new NotImplementedException();
		}

		/// <summary>
		/// Constructs an IRezolver
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="newExpr"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
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

		public static IRezolveTarget Best<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return new BestMatchConstructorTarget(typeof(T), propertyBindingBehaviour);
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