// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// A target that binds to a type's constructor with zero or more arguments supplied by other <see cref="ITarget"/>s and, optionally
	/// binding to the new instance's writeable properties.
	/// </summary>
	/// <remarks>Although you can create this target directly through the 
	/// <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, IPropertyBindingBehaviour, ParameterBinding[])"/> constructor,
	/// you're more likely to create it through factory methods such as <see cref="Auto{T}(IPropertyBindingBehaviour)"/> or, more likely still,
	/// extension methods such as <see cref="ITargetContainerExtensions.RegisterType{TObject, TService}(ITargetContainer, IPropertyBindingBehaviour)"/> during
	/// your application's container setup phase.
	/// 
	/// The expression built by this class' implementation of <see cref="CreateExpressionBase(CompileContext)"/> will be an expression tree that ultimately
	/// creates a new instance of the <see cref="DeclaredType"/>.</remarks>
	public partial class ConstructorTarget : TargetBase
	{
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
			private readonly ParameterBinding[] _boundArgs;

			public BoundConstructorTarget(ConstructorInfo ctor, params ParameterBinding[] boundArgs)
			{
				_ctor = ctor;
				_boundArgs = boundArgs ?? ParameterBinding.None;
			}

			public Expression CreateExpression(CompileContext context)
			{
				return Expression.New(_ctor, _boundArgs.Select(a => a.CreateExpression(context)));
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
					var nestedExpression = _nestedTarget.CreateExpression(context.New(_nestedTarget.DeclaredType));
					//have to locate the NewExpression constructed by the inner target and then rewrite it as
					//a MemberInitExpression with the given property bindings.  Note - if the expression created
					//by the ConstructorTarget is surrounded with any non-standard funny stuff - i.e. anything that
					//could require a NewExpression, then this code won't work.  Points to the possibility that we
					//might need some additional funkiness to allow code such as this to do its thing.
					return new NewExpressionMemberInitRewriter(null,
						_propertyBindings.Select(b => b.CreateMemberBinding(context.New(b.MemberType)))).Visit(nestedExpression);
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

		private static readonly IDictionary<string, ITarget> _emptyArgsDictionary = new Dictionary<string, ITarget>();

		private readonly ConstructorInfo _ctor;

		/// <summary>
		/// Can be null. Gets the constructor that this target is bound to, if known at construction time.
		/// </summary>
		/// <remarks>ConstructorTargets can be bound to a particular constructor
		/// in advance, or they can search for a best-match constructor at the point where
		/// <see cref="ITarget.CreateExpression(CompileContext)"/> is called.
		/// 
		/// This property will only be set ultimately if it was passed to the 
		/// <see cref="ConstructorTarget.ConstructorTarget(ConstructorInfo, IPropertyBindingBehaviour, ParameterBinding[])"/>
		/// constructor, possibly by a factory method like <see cref="ConstructorTarget.WithArgs(ConstructorInfo, IDictionary{string, ITarget})"/>,
		/// or <see cref="ConstructorTarget.FromNewExpression(Type, NewExpression, ITargetAdapter)"/>, where the constructor
		/// is captured within the expression.</remarks>
 		public ConstructorInfo Ctor
		{
			get
			{
				return _ctor;
			}
		}

		private readonly ParameterBinding[] _parameterBindings;

		/// <summary>
		/// If this target was created with a specific constructor then this might contain
		/// argument bindings for that constructor's parameters.
		/// </summary>
		/// <remarks>This is not the same as <see cref="NamedArgs"/> - as is noted by the documentation
		/// on that property.  This property is for when the constructor is known in advance; whereas
		/// <see cref="NamedArgs"/> is for when it's not.</remarks>
		public IReadOnlyList<ParameterBinding> ParameterBindings
		{
			get
			{
				return _parameterBindings;
			}
		}

		private IPropertyBindingBehaviour _propertyBindingBehaviour;

		private readonly IDictionary<string, ITarget> _namedArgs;

		/// <summary>
		/// Named arguments (as <see cref="ITarget"/> objects) to be supplied to the object on construction,
		/// also aiding the search for a constructor.
		/// </summary>
		/// <remarks>Note the difference between this and <see cref="ParameterBindings"/> - this
		/// property might be used when the constructor is not known in advance, whereas 
		/// <see cref="ParameterBindings"/> is used when it is.</remarks>
		public IReadOnlyDictionary<string, ITarget> NamedArgs
		{
			get
			{
				return new DictionaryReadOnlyWrapper<string, ITarget>(_namedArgs);
			}
		}

		private readonly Type _declaredType;
		/// <summary>
		/// Implementation of <see cref="TargetBase.DeclaredType"/>.  Always equal to the
		/// type whose constructor will be bound by this target.
		/// </summary>
		public override Type DeclaredType
		{
			get { return _declaredType; }
		}

		/// <summary>
		/// Initializes a late-bound instance of the <see cref="ConstructorTarget" /> class which will locate the
		/// best constructor to be called at compile-time.
		/// </summary>
		/// <param name="type">Required.  The type to be constructed when resolved in a container.</param>
		/// <param name="propertyBindingBehaviour">Optional.  If provided, can be used to select properties which are to be
		/// initialised from the container.</param>
		/// <param name="namedArgs">Optional.  The named arguments which will be provided to the best-matched constructor.  These are taken into account
		/// when the constructor is sought - with the constructor that the most parameters matched being selected.</param>
		/// <remarks>The best available constructor on the <paramref name="type" /> is determined
		/// when <see cref="ITarget.CreateExpression(CompileContext)" /> is called (which ultimately calls <see cref="CreateExpressionBase(CompileContext)" />).
		/// The best available constructor is defined as the constructor with the most parameters for which arguments can be resolved from the <see cref="CompileContext" /> at compile-time
		/// (i.e. when <see cref="CreateExpressionBase(CompileContext)" /> is called) to the fewest number of <see cref="ITarget" /> objects whose <see cref="ITarget.UseFallback" />
		/// is false (for example - when an IEnumerable of a service is requested, but no registrations are found, a target is returned with <see cref="ITarget.UseFallback" /> set to
		/// <c>true</c>, and whose expression will equate to an empty enumerable).
		/// This allows the system to bind to different constructors automatically based on the other registrations that are present in the <see cref="ITargetContainer" /> of the active
		/// <see cref="RezolveContext.Container" /> when code is compiled in response to a call to <see cref="IContainer.Resolve(RezolveContext)" />.</remarks>
		public ConstructorTarget(Type type, IPropertyBindingBehaviour propertyBindingBehaviour = null, IDictionary<string, ITarget> namedArgs = null)
			: this(type, null, propertyBindingBehaviour, null, namedArgs)
		{
			//it's a post-check, but the private constructor sidesteps null types and ctors to allow the
			//public constructors to do their thing.
			type.MustNotBeNull(nameof(type));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorTarget"/> class.
		/// </summary>
		/// <param name="ctor">Required - the constructor that is to be bound.  The <see cref="DeclaredType"/> of the new instance
		/// will be derived from this.</param>
		/// <param name="propertyBindingBehaviour">Optional.  If provided, can be used to select properties which are to be
		/// initialised from the container.</param>
		/// <param name="parameterBindings">Optional, although can only be supplied if <paramref name="ctor"/> is provided.  
		/// Specific bindings for the parameters of the given <paramref name="ctor"/> which should be used during code generation.</param>
		public ConstructorTarget(ConstructorInfo ctor, IPropertyBindingBehaviour propertyBindingBehaviour = null, ParameterBinding[] parameterBindings = null)
			: this(null, ctor, propertyBindingBehaviour, parameterBindings, null)
		{
			ctor.MustNotBeNull(nameof(ctor));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorTarget"/> class.
		/// 
		/// Used only by other constructors and some factory methods.
		/// 
		/// Has no argument-checking logic.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="ctor">The ctor.</param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
		/// <param name="parameterBindings">The parameter bindings.</param>
		/// <param name="suppliedArgs">The supplied arguments.</param>
		private ConstructorTarget(Type type, 
			ConstructorInfo ctor, 
			IPropertyBindingBehaviour propertyBindingBehaviour, 
			ParameterBinding[] parameterBindings, 
			IDictionary<string, ITarget> suppliedArgs)
		{
			_ctor = ctor;
			_declaredType = type ?? (ctor != null ? ctor.DeclaringType : null);
			if (type != null)
			{
				type.MustNot(t => TypeHelpers.IsInterface(t) || TypeHelpers.IsAbstract(t), "Type must not be an interface or an abstract class", nameof(type));
			}
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
			_propertyBindingBehaviour = propertyBindingBehaviour;
			_namedArgs = suppliedArgs ?? new Dictionary<string, ITarget>();
		}

		/// <summary>
		/// Implementation of <see cref="TargetBase.CreateExpressionBase(CompileContext)"/>
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <exception cref="AmbiguousMatchException">If no definitively 'best' constructor can be determined.</exception>
		protected override Expression CreateExpressionBase(CompileContext context)
		{
			ConstructorInfo ctor = _ctor;
			ParameterBinding[] boundArgs = ParameterBinding.None;

			if (ctor == null)
			{
				//have to go searching for the best constructor match for the current context,
				//which will also give us our arguments
				var publicCtorGroups = GetPublicConstructorGroups(DeclaredType);
				//var possibleBindingsGrouped = publicCtorGroups.Select(g => g.Select(ci => new BoundConstructorTarget(ci, ParameterBinding.BindMethod(ci))));
				var ctorsWithBindingsGrouped = publicCtorGroups.Select(g =>
				  g.Select(ci => new
				  {
					  ctor = ci,
					  //filtered collection of parameter bindings along with the actual ITarget that is resolved for each
					  //NOTE: we're using the default behaviour of ParameterBinding here which is to auto-resolve an argument
					  //value or to use the parameter's default if it is optional.
					  bindings = ParameterBinding.BindMethod(ci, _namedArgs)// ci.GetParameters().Select(pi => new ParameterBinding(pi))
					  .Select(pb => new { Parameter = pb, RezolvedArg = pb.Resolve(context) })
					  .Where(bp => bp.RezolvedArg != null).ToArray()
					  //(ABOVE) only include bindings where a target was found - means we can quickly
					  //determine if all parameters are bound by checking the array length is equal to the
					  //number of parameters on the constructor itself (BELOW)
				  }).Where(a => a.bindings.Length == g.Key).ToArray()
				).Where(a => a.Length > 0).ToArray(); //filter to where there is at least one successfully bound constructor

				//No constructors for which we could bind all parameters with either a mix of resolved or default arguments.
				//so we'll auto-bind to the constructor with the most parameters - if there is one - leaving the application
				//with the responsibility of ensuring that the correct registrations are made in the target container, or 
				//in the container supplied at resolve-time, to satisfy the constructor's dependencies.
				if (ctorsWithBindingsGrouped.Length == 0)
				{
					if (publicCtorGroups.Length != 0)
					{
						var mostGreedy = publicCtorGroups[0].ToArray();
						if (mostGreedy.Length > 1)
							throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneConstructorFormat, DeclaredType, string.Join(", ", mostGreedy.AsEnumerable())));
						else
						{
							ctor = mostGreedy[0];
							boundArgs = ParameterBinding.BindWithRezolvedArguments(ctor).ToArray();
						}
					}
					else
						throw new InvalidOperationException(string.Format(ExceptionResources.NoApplicableConstructorForContextFormat, _declaredType));
				}
				else //managed to bind at least constructor up front to registered targets or defaults
				{
					//get the greediest constructors with successfully bound parameters.
					var mostBound = ctorsWithBindingsGrouped[0];
					//get the first result
					var toBind = mostBound[0];
					//if there is only one, then we can move on to code generation
					if (mostBound.Length > 1)
					{
						//the question now is one of disambiguation: 
						//choose the one with the fewest number of targets with ITarget.UseFallback set to true
						//if we still can't disambiguate, then we have an exception.
						var fewestFallback = mostBound.GroupBy(a => a.bindings.Count(b => b.RezolvedArg.UseFallback)).FirstOrDefault().ToArray();
						if (fewestFallback.Length > 1)
							throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneBestConstructorFormat, DeclaredType, string.Join(", ", fewestFallback.Select(a => a.ctor))));
						toBind = fewestFallback[0];
					}
					ctor = toBind.ctor;
					boundArgs = toBind.bindings.Select(a => a.Parameter).ToArray();
				}
			}
			//we allow for no parameter bindings to be provided on construction, and have them dynamically determined
			else if (_parameterBindings.Length == 0 && ctor.GetParameters().Length != 0)
			{
				//just need to generate the bound parameters - nice and easy
				//because the constructor was provided up-front, we don't check whether the target can be resolved
				boundArgs = ParameterBinding.BindMethod(ctor, _namedArgs);// ParameterBinding.BindWithRezolvedArguments(ctor);
			}
			else
				boundArgs = _parameterBindings;

			BoundConstructorTarget baseTarget = new BoundConstructorTarget(ctor, boundArgs);
			ITarget target = null;
			if (_propertyBindingBehaviour != null)
				target = new MemberInitialiserTarget(baseTarget, _propertyBindingBehaviour.GetPropertyBindings(context, _declaredType));
			else
				target = baseTarget;
			return target.CreateExpression(context);
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
	}
}