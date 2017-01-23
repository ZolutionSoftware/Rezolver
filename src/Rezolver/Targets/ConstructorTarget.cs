// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
	/// <summary>
	/// A target that binds to a type's constructor with zero or more arguments supplied by other <see cref="ITarget"/>s and, optionally
	/// binding to the new instance's writeable properties.
	/// 
	/// The target supports en explicitly supplied constructor, as well as just-in-time lookups for the best available constructor based on the
	/// available services and/or any named arguments which might been provided up front.
	/// </summary>
	/// <remarks>Although you can create this target directly through the 
	/// <see cref="ConstructorTarget.ConstructorTarget(Type, ConstructorInfo, IMemberBindingBehaviour, ParameterBinding[], IDictionary{string, ITarget})"/> constructor,
	/// you're more likely to create it through factory methods such as <see cref="Auto{T}(IMemberBindingBehaviour)"/> or, more likely still,
	/// extension methods such as <see cref="RegisterTypeTargetContainerExtensions.RegisterType{TObject, TService}(ITargetContainer, IMemberBindingBehaviour)"/> during
	/// your application's container setup phase.
	/// 
	/// To compile this target, an <see cref="Compilation.ITargetCompiler"/> must call the <see cref="Bind(ICompileContext)"/> method 
	/// </remarks>
	public partial class ConstructorTarget : TargetBase
	{
		private readonly ConstructorInfo _ctor;

		/// <summary>
		/// Can be null. Gets the constructor that this target is bound to, if it was known when the target
		/// was created.
		/// </summary>
		/// <remarks>ConstructorTargets can be bound to a particular constructor
		/// in advance, or they can search for a best-match constructor at the point where
		/// <see cref="Bind(ICompileContext)"/> is called.
		/// 
		/// This property will only be set ultimately if it was passed to the 
		/// <see cref="ConstructorTarget.ConstructorTarget(ConstructorInfo, IMemberBindingBehaviour, ParameterBinding[])"/>
		/// constructor, possibly by a factory method like <see cref="ConstructorTarget.WithArgs(ConstructorInfo, IDictionary{string, ITarget})"/>,
		/// or <see cref="ConstructorTarget.FromNewExpression(Type, NewExpression)"/>, where the constructor
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
		/// on that property.  This property is for when the constructor is known in advance and when certain
		/// parameters are to be bound with specific arguments; whereas
		/// <see cref="NamedArgs"/> is for when the constructor is not known.</remarks>
		public IReadOnlyList<ParameterBinding> ParameterBindings
		{
			get
			{
				return _parameterBindings;
			}
		}

		/// <summary>
		/// Gets the member binding behaviour to be used when <see cref="Bind(ICompileContext)"/> is called.
		/// </summary>
		public IMemberBindingBehaviour MemberBindingBehaviour
		{
			get; private set;
		}

		private readonly IDictionary<string, ITarget> _namedArgs;
		/// <summary>
		/// Named arguments (as <see cref="ITarget"/> objects) to be supplied to the object on construction,
		/// also aiding the search for a constructor.  This equivalent to the way that languages such as C# 
		/// can bind to overloaded functions based solely on named arguments.
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

		/// <summary>
		/// Implementation of <see cref="TargetBase.DeclaredType"/>.  Always equal to the
		/// type whose constructor will be bound by this target.
		/// </summary>
		public override Type DeclaredType { get; }

		/// <summary>
		/// Initializes a just-in-time-bound instance of the <see cref="ConstructorTarget" /> class which must be bound 
		/// to the best constructor at compile-time by calling the <see cref="Bind(ICompileContext)"/> method.
		/// </summary>
		/// <param name="type">Required.  The type whose constructor is to bound.</param>
		/// <param name="memberBindingBehaviour">Optional.  If provided, can be used to select properties which are to be
		/// initialised before the new instance is returned.</param>
		/// <param name="namedArgs">Optional.  The named arguments which will be passed to, and used to find, the best-matched constructor.  
		/// These are taken into account when the constructor is sought - with the constructor containing the most matched parameters matched being selected.</param>
		/// <remarks>To compile this target, a <see cref="Compilation.ITargetCompiler"/> first calls the <see cref="Bind(ICompileContext)"/> method
		/// to discover the constructor to be executed, along with the final set of arguments to be provided to it (see <see cref="ConstructorBinding"/>).
		/// 
		/// The best available constructor is defined as the constructor with the most parameters for which arguments can be resolved from the 
		/// <see cref="ICompileContext" /> at compile-time to the fewest number of <see cref="ITarget" /> objects whose <see cref="ITarget.UseFallback" />
		/// is false.
		/// </remarks>
		public ConstructorTarget(Type type, IMemberBindingBehaviour memberBindingBehaviour = null, IDictionary<string, ITarget> namedArgs = null)
			: this(type, null, memberBindingBehaviour, null, namedArgs)
		{
			//it's a post-check, but the private constructor sidesteps null types and ctors to allow the
			//public constructors to do their thing.
			type.MustNotBeNull(nameof(type));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorTarget"/> class bound in advance to a specific constructor.
		/// </summary>
		/// <param name="ctor">Required - the constructor that is to be bound.  The <see cref="DeclaredType"/> of the new instance
		/// will be set to the <see cref="MemberInfo.DeclaringType"/> of this object.</param>
		/// <param name="memberBindingBehaviour">Optional.  If provided, can be used to select properties which are to be
		/// initialised before the new instance is returned.</param>
		/// <param name="parameterBindings">Optional.  Specific bindings for the parameters of the given <paramref name="ctor"/>
		/// which should be used during code generation.  Note that this array can contain fewer or more entries than there are
		/// parameters on the <paramref name="ctor"/>.  Any missing bindings will be automatically generated when <see cref="Bind(ICompileContext)"/>
		/// is called.</param>
		public ConstructorTarget(ConstructorInfo ctor, IMemberBindingBehaviour memberBindingBehaviour = null, ParameterBinding[] parameterBindings = null)
			: this(null, ctor, memberBindingBehaviour, parameterBindings, null)
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
		/// <param name="memberBindingBehaviour">The property binding behaviour.</param>
		/// <param name="parameterBindings">The parameter bindings.</param>
		/// <param name="suppliedArgs">The supplied arguments.</param>
		private ConstructorTarget(Type type,
			ConstructorInfo ctor,
			IMemberBindingBehaviour memberBindingBehaviour,
			ParameterBinding[] parameterBindings,
			IDictionary<string, ITarget> suppliedArgs)
		{
			_ctor = ctor;
			DeclaredType = type ?? (ctor != null ? ctor.DeclaringType : null);
			if (type != null)
			{
				type.MustNot(t => TypeHelpers.IsInterface(t) || TypeHelpers.IsAbstract(t), "Type must not be an interface or an abstract class", nameof(type));
			}
			_parameterBindings = parameterBindings ?? ParameterBinding.None;
			MemberBindingBehaviour = memberBindingBehaviour;
			_namedArgs = suppliedArgs ?? new Dictionary<string, ITarget>();
		}

		/// <summary>
		/// Gets the <see cref="ConstructorBinding"/> for the <see cref="DeclaredType"/> using the 
		/// targets available in the <paramref name="context"/> for dependency lookup.
		/// 
		/// The constructor is either resolved by checking available targets for the best match, or is pre-selected
		/// on construction (<see cref="Ctor"/> will be non-null in this case).
		/// </summary>
		/// <param name="context">The current compilation context.</param>
		/// <exception cref="AmbiguousMatchException">If more than one constructor can be bound with an equal amount of all-resolved
		/// arguments or default arguments.</exception>
		/// <exception cref="InvalidOperationException">If no sutiable constructors can be found.</exception>
		public ConstructorBinding Bind(ICompileContext context)
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

				if (ctorsWithBindingsGrouped.Length == 0)
				{
					//No constructors for which we could bind all parameters with either a mix of resolved or default arguments.
					//so we'll auto-bind to the constructor with the most parameters - if there is one - leaving the application
					//with the responsibility of ensuring that the correct registrations are made in the target container, or 
					//in the container supplied at resolve-time, to satisfy the constructor's dependencies.
					if (publicCtorGroups.Length != 0)
					{
						var mostGreedy = publicCtorGroups[0].ToArray();
						if (mostGreedy.Length > 1)
						{
							//see if we can get a single constructor which has the fewest number of optionals
							// - even though we haven't got configured services for all (or even any), we can choose
							// one based on the greedy rule.
							var leastOptional = mostGreedy.Select(c => new {  ctor = c, optionalCount = c.GetParameters().Count(p  => p.IsOptional)})
								.OrderBy(c => c.optionalCount)
								.GroupBy(c => c.optionalCount)
								.First()
								.ToArray();
							if (leastOptional.Length == 1)
							{
								ctor = leastOptional[0].ctor;
								boundArgs = ParameterBinding.BindWithRezolvedArguments(ctor).ToArray();
							}
							else
								throw new AmbiguousMatchException(string.Format(ExceptionResources.MoreThanOneConstructorFormat, DeclaredType, string.Join(", ", mostGreedy.AsEnumerable())));
						}
						else
						{
							ctor = mostGreedy[0];
							boundArgs = ParameterBinding.BindWithRezolvedArguments(ctor).ToArray();
						}
					}
					else
						throw new InvalidOperationException(string.Format(ExceptionResources.NoApplicableConstructorForContextFormat, DeclaredType));
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
			//also allow for some to be ommitted (check by testing reference equality on the parameterinfo)
			else if (_parameterBindings.Length != ctor.GetParameters().Length
				|| !ctor.GetParameters().All(p => _parameterBindings.FirstOrDefault(pb => pb.Parameter == p) != null))
			{
				//just need to generate the bound parameters - nice and easy
				//because the constructor was provided up-front, we don't check whether the target can be resolved
				boundArgs = ParameterBinding.BindMethod(ctor, _parameterBindings);// ParameterBinding.BindWithRezolvedArguments(ctor);
			}
			else
				boundArgs = _parameterBindings;

			return new ConstructorBinding(ctor, boundArgs, MemberBindingBehaviour?.GetMemberBindings(context, DeclaredType));
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