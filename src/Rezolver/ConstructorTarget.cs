using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// The most common target used for building new objects.  Represents binding to a type's constructor with zero or more
	/// arguments supplied by other <see cref="ITarget"/>s.
	/// 
	/// In addition to using the <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, ParameterBinding[])"/> constructor
	/// you can also use the factory methods - such as <see cref="Auto{T}(IPropertyBindingBehaviour)"/> and <see cref="Best{T}(IPropertyBindingBehaviour)"/>.
	/// </summary>
	public class ConstructorTarget : TargetBase
	{
		private class BoundArgument
		{
			public ParameterInfo Parameter { get; }
			public ITarget Argument { get; }

			public BoundArgument(ParameterInfo parameter, ITarget argument)
			{
				Parameter = parameter;
				Argument = argument;
			}

			public Expression CreateExpression(CompileContext parentContext)
			{
				//note - we don't switch off scope tracking here - it's up to the individual targets to determine if they need 
				//to do that.
				return Argument.CreateExpression(new CompileContext(parentContext, Parameter.ParameterType, inheritSharedExpressions: true));
			}
		}

		//note - neither of these two classes inherit from TargetBase because that class performs lots 
		//of unnecessary checks that don't apply to these because we always create boundconstructortargets from 
		//within the ConstructorTarget's CreateExpressionBase - so they effectively inherit it's TargetBase
		private class BoundConstructorTarget : ITarget
		{
			public Type DeclaredType
			{
				get
				{
					return _ctor.DeclaringType;
				}
			}

			public bool UseFallback
			{
				get
				{
					return false;
				}
			}

			private readonly ConstructorInfo _ctor;
			private readonly BoundArgument[] _boundArgs;

			public BoundConstructorTarget(ConstructorInfo ctor, params BoundArgument[] boundArgs)
			{
				_ctor = ctor;
				_boundArgs = boundArgs ?? new BoundArgument[0];
			}

			public Expression CreateExpression(CompileContext context)
			{
				return Expression.New(_ctor, _boundArgs.Select(t => t.CreateExpression(context)));
			}

			public bool SupportsType(Type type)
			{
				return TypeHelpers.AreCompatible(DeclaredType, type);
			}
		}

		private class MemberInitialiserTarget : ITarget
		{
			PropertyOrFieldBinding[] _propertyBindings;
			BoundConstructorTarget _nestedTarget;

			/// <summary>
			/// Please note that with this constructor, no checking is performed that the property bindings are
			/// actually valid for the target's type.
			/// </summary>
			/// <param name="target"></param>
			/// <param name="propertyBindings"></param>
			internal MemberInitialiserTarget(BoundConstructorTarget target, PropertyOrFieldBinding[] propertyBindings)
				: base()
			{
				_nestedTarget = target;
				_propertyBindings = propertyBindings ?? PropertyOrFieldBinding.None;
			}

			public bool SupportsType(Type type)
			{
				return _nestedTarget.SupportsType(type);
			}

			public Expression CreateExpression(CompileContext context)
			{
				if (_propertyBindings.Length == 0)
					return _nestedTarget.CreateExpression(context);
				else
				{
					var nestedExpression = _nestedTarget.CreateExpression(new CompileContext(context, _nestedTarget.DeclaredType, true));
					//have to locate the NewExpression constructed by the inner target and then rewrite it as
					//a MemberInitExpression with the given property bindings.  Note - if the expression created
					//by the ConstructorTarget is surrounded with any non-standard funny stuff - i.e. anything that
					//could require a NewExpression, then this code won't work.  Points to the possibility that we
					//might need some additional funkiness to allow code such as this to do its thing.
					return new NewExpressionMemberInitRewriter(null,
							_propertyBindings.Select(b => b.CreateMemberBinding(new CompileContext(context, b.MemberType, true)))).Visit(nestedExpression);
				}
			}

			public Type DeclaredType
			{
				get { return _nestedTarget.DeclaredType; }
			}

			public bool UseFallback
			{
				get
				{
					return _nestedTarget.UseFallback;
				}
			}
		}
		/// <summary>
		/// A target that helps a constructor target bind to the best matching constructor on a type
		/// given a CompileContext's registered targets.
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
				var ctorsWithBindingsGrouped = GetPublicConstructorGroups(DeclaredType).Select(g =>
					g.Select(ci => new
					{
						ctor = ci,
						//filtered collection of parameter bindings along with the actual ITarget that is resolved for each
						//NOTE: we're using the default behaviour of ParameterBinding here which is to auto-resolve an argument
						//value or to use the parameter's default if it is optional.
						bindings = ci.GetParameters().Select(pi => new ParameterBinding(pi))
							.Select(pb => new BoundArgument(pb.Parameter, pb.Resolve(context)))
							.Where(bp => bp.Argument != null).ToArray()
						//(ABOVE) only include bindings where a target was found - means we can quickly
						//determine if all parameters are bound by checking the array length is equal to the
						//number of parameters on the constructor itself (BELOW)
					}).Where(a => a.bindings.Length == g.Key).ToArray()
				).Where(a => a.Length > 0).ToArray(); //filter to where there is at least one successfully bound constructor
				
				//No constructors for which we could bind all parameters with either a mix of resolved or default arguments.
				if (ctorsWithBindingsGrouped.Length == 0)
					throw new InvalidOperationException(string.Format(ExceptionResources.NoApplicableConstructorForContextFormat, _declaredType));

				//get the greediest constructors with successfully bound parameters.
				var mostBound = ctorsWithBindingsGrouped[0];
				//get the first result
				var toBind = mostBound[0];
				//if there is only one, then we can move on to code generation.
				
				if (mostBound.Length > 1)
				{
					//the question now is one of disambiguation: 
					//choose the one with the fewest number of targets with ITarget.UseFallback set to true
					//if we still can't disambiguate, then we have an exception.
					var fewestFallback = mostBound.GroupBy(a => a.bindings.Count(b => b.Argument.UseFallback)).FirstOrDefault().ToArray();
					if (fewestFallback.Length > 1)
						throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneBestConstructorFormat, DeclaredType, string.Join(", ", fewestFallback.Select(a => a.ctor))));
					toBind = fewestFallback[0];
				}

				//TODO: change this to supply the rezolved targets to the constructor
				//also, consider creating a child class which has no search semantics at all - which simply takes the 
				//type/constructor and targets for the parameters (in order) and emits an expression for that only.
				var baseTarget = new BoundConstructorTarget(toBind.ctor, toBind.bindings);
				ITarget target = null;
				if (_propertyBindingBehaviour != null)
					target = new MemberInitialiserTarget(baseTarget, _propertyBindingBehaviour.GetPropertyBindings(context, _declaredType));
				else
					target = baseTarget;
				//we force scope tracking off because our base class will generate it for us
				return target.CreateExpression(new CompileContext(context, inheritSharedExpressions: true, suppressScopeTracking: true));
			}
		}

		private static readonly Type[] EmptyTypes = new Type[0];

		private readonly Type _declaredType;
		private readonly ConstructorInfo _ctor;
		private readonly ParameterBinding[] _parameterBindings;
		private IPropertyBindingBehaviour _propertyBindingBehaviour;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="ctor"></param>
		/// <param name="propertyBindingBehaviour"></param>
		/// <param name="parameterBindings"></param>
		public ConstructorTarget(Type type, ConstructorInfo ctor, IPropertyBindingBehaviour propertyBindingBehaviour, ParameterBinding[] parameterBindings)
		{
			_declaredType = type;
			_ctor = ctor;
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
			_propertyBindingBehaviour = propertyBindingBehaviour;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			ConstructorInfo ctor = _ctor;
			BoundArgument[] boundArgs = new BoundArgument[0];

			if (ctor == null)
			{
				//have to go searching for the best constructor match for the current context,
				//which will also give us our arguments
				var publicCtorGroups = GetPublicConstructorGroups(DeclaredType);
				var ctorsWithBindingsGrouped = publicCtorGroups.Select(g =>
					g.Select(ci => new
					{
						ctor = ci,
						//filtered collection of parameter bindings along with the actual ITarget that is resolved for each
						//NOTE: we're using the default behaviour of ParameterBinding here which is to auto-resolve an argument
						//value or to use the parameter's default if it is optional.
						bindings = ci.GetParameters().Select(pi => new ParameterBinding(pi))
							.Select(pb => new { Parameter = pb, BoundArg = new BoundArgument(pb.Parameter, pb.Resolve(context)) })
							.Where(bp => bp.BoundArg.Argument != null).ToArray()
						//(ABOVE) only include bindings where a target was found - means we can quickly
						//determine if all parameters are bound by checking the array length is equal to the
						//number of parameters on the constructor itself (BELOW)
					}).Where(a => a.bindings.Length == g.Key).ToArray()
				).Where(a => a.Length > 0).ToArray(); //filter to where there is at least one successfully bound constructor

				//No constructors for which we could bind all parameters with either a mix of resolved or default arguments.
				//so we'll auto-bind to the constructor with the most parameters - if there is one
				if (ctorsWithBindingsGrouped.Length == 0)
				{
					if (publicCtorGroups.Length != 0)
					{
						var mostGreedy = publicCtorGroups[0].ToArray();
						if (mostGreedy.Length > 1)
							throw new InvalidOperationException(string.Format(ExceptionResources.MoreThanOneConstructorFormat, DeclaredType));
						else
						{
							ctor = mostGreedy[0];
							boundArgs = ParameterBinding.BindWithRezolvedArguments(ctor).Select(pb => new BoundArgument(pb.Parameter, pb.Target)).ToArray();
						}
					}
					else
						throw new InvalidOperationException(string.Format(ExceptionResources.NoApplicableConstructorForContextFormat, _declaredType));
				}
				else
				{

					//get the greediest constructors with successfully bound parameters.
					var mostBound = ctorsWithBindingsGrouped[0];
					//get the first result
					var toBind = mostBound[0];
					//if there is only one, then we can move on to code generation.

					if (mostBound.Length > 1)
					{
						//the question now is one of disambiguation: 
						//choose the one with the fewest number of targets with ITarget.UseFallback set to true
						//if we still can't disambiguate, then we have an exception.
						var fewestFallback = mostBound.GroupBy(a => a.bindings.Count(b => b.BoundArg.Argument.UseFallback)).FirstOrDefault().ToArray();
						if (fewestFallback.Length > 1)
							throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneBestConstructorFormat, DeclaredType, string.Join(", ", fewestFallback.Select(a => a.ctor))));
						toBind = fewestFallback[0];
					}
					ctor = toBind.ctor;
					boundArgs = toBind.bindings.Select(a => new BoundArgument(a.Parameter.Parameter, a.Parameter.Target)).ToArray();
				}
			}
			//we allow for no parameter bindings to be provided on construction, and have them dynamically determined
			else if (_parameterBindings.Length == 0 && ctor.GetParameters().Length != 0)
			{
				//just need to generate the bound parameters - nice and easy
				var tempBindings = ParameterBinding.BindWithRezolvedArguments(ctor);
				//because the constructor was provided up-front, we don't check whether the target can be resolved
				boundArgs = tempBindings.Select(pb => new BoundArgument(pb.Parameter, pb.Target)).ToArray();
			}
			else
				boundArgs = _parameterBindings.Select(pb => new BoundArgument(pb.Parameter, pb.Target)).ToArray();

			BoundConstructorTarget baseTarget = new BoundConstructorTarget(ctor, boundArgs);
			ITarget target = null;
			if (_propertyBindingBehaviour != null)
				target = new MemberInitialiserTarget(baseTarget, _propertyBindingBehaviour.GetPropertyBindings(context, _declaredType));
			else
				target = baseTarget;
			//we force scope tracking off because our base class will generate it for us
			return target.CreateExpression(new CompileContext(context, inheritSharedExpressions: true, suppressScopeTracking: true));
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

			return new ConstructorTarget(type, null, propertyBindingBehaviour, null);
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
		/// Constructs an constructor target that's reconstructed from a <see cref="System.Linq.Expressions.NewExpression"/>, including any
		/// parameter bindings.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="newExpr"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		public static ITarget FromNewExpression<T>(Expression<Func<RezolveContextExpressionHelper, T>> newExpr, ITargetAdapter adapter = null)
		{
			newExpr.MustNotBeNull(nameof(newExpr));
			NewExpression newExprBody = null;

			newExprBody = newExpr.Body as NewExpression;
			if (newExprBody == null)
				throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyIsNotNewExpressionFormat, newExpr), nameof(newExpr));
			else if (newExprBody.Type != typeof(T))
				throw new ArgumentException(string.Format(ExceptionResources.LambdaBodyNewExpressionIsWrongTypeFormat, newExpr, typeof(T)), nameof(newExpr));

			return FromNewExpression(typeof(T), newExprBody, adapter);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="declaredType"></param>
		/// <param name="newExpr"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		public static ITarget FromNewExpression(Type declaredType, NewExpression newExpr, ITargetAdapter adapter = null)
		{
			ConstructorInfo ctor = null;
			ParameterBinding[] parameterBindings = null;

			ctor = newExpr.Constructor;
			parameterBindings = ExtractParameterBindings(newExpr, adapter ?? TargetAdapter.Default).ToArray();

			return new ConstructorTarget(declaredType, ctor, null, parameterBindings);
		}

		public static ITarget Best<T>(IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return Best(typeof(T), propertyBindingBehaviour);
		}

		public static ITarget Best(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			return new ConstructorTarget(type, null, propertyBindingBehaviour, null);
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

			return new ConstructorTarget(declaredType, ctor, null, bindings);
		}

		internal static ITarget WithArgsInternal(Type declaredType, IDictionary<string, ITarget> args)
		{
			MethodBase ctor = null;
			var bindings = ParameterBinding.BindOverload(TypeHelpers.GetConstructors(declaredType), args, out ctor);

			return new ConstructorTarget(declaredType, (ConstructorInfo)ctor, null, bindings);
		}



		private static IEnumerable<ParameterBinding> ExtractParameterBindings(NewExpression newExpr, ITargetAdapter adapter)
		{
			return newExpr.Constructor.GetParameters()
				.Zip(newExpr.Arguments, (info, expression) => new ParameterBinding(info, adapter.CreateTarget(expression))).ToArray();
		}
	}
}