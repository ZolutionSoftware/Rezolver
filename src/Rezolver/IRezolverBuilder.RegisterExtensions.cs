using Rezolver.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Extension methods for implementations of <see cref="IRezolverBuilder"/>.
	/// </summary>
	public static partial class IRezolverBuilderExtensions
	{
		/// <summary>
		/// Registers an expression to be used as a factory for obtaining an instance when the registration matches a resolve request.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="builder"></param>
		/// <param name="expression">The expression to be analysed and used as a factory.  The argument that is received by this expression can be used to emit explicit
		/// calls back into the resolver to indicate that a particular argument/property value or whatever should be resolved.</param>
		/// <param name="type">Optional.  The type against which the registration is to be made, if different from <typeparamref name="T"/>.</param>
		/// <param name="path">Optional.  Same as in <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/></param>
		/// <param name="adapter">Optional.  The adapter that will be used to convert the <paramref name="expression"/> into an <see cref="IRezolveTarget"/>.  This defaults
		/// to <see cref="RezolveTargetAdapter.Default"/>.  Extending this is an advanced topicand shouldn't be required in most applications - it's mainly for developers looking
		/// to extend Rezolver itself.</param>
		/// <remarks>This is not the same as registering a factory delegate for creating objects - where the code you supply is already compiled and ready to go.  The expression that
		/// is passed is analysed by the <paramref name="adapter"/> (or the default) and rewritten according to the expressions present.  In general, there is a one to one
		/// mapping between the code you provide and the code that's produced, but it's not guaranteed.  In particular, calls back to the resolver to resolve dependencies are
		/// identified and turned into a different representation internally, so that dependency resolution works inside your code in just the same way as it does when using the
		/// higher-level targets.</remarks>
		public static void RegisterExpression<T>(this IRezolverBuilder builder, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, RezolverPath path = null, IRezolveTargetAdapter adapter = null)
		{
			builder.MustNotBeNull("builder");
			expression.MustNotBeNull("expression");
			var target = (adapter ?? RezolveTargetAdapter.Default).GetRezolveTarget(expression);
			builder.Register(target, type ?? typeof(T), path);
		}

		/// <summary>
		/// Called to register multiple rezolve targets against a shared contract, optionally replacing any 
		/// existing registration(s) or extending them.
		/// 
		/// It is analogous to calling <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/> multiple times
		/// with the different targets.
		/// </summary>
		/// <param name="builder">The builder in which the registration is to be performed.</param>
		/// <param name="targets">The targets to be registered - all must support a common service type (potentially
		/// passed in the <paramref name="commonServiceType"/> argument.</param>
		/// <param name="commonServiceType">Optional - instead of determining the common service type automatically,
		/// you can provide it in advance through this parameter.  Note that all targets must support this type.</param>
		/// <param name="path">Optional path under which this registration is to be made.</param>
		public static void RegisterMultiple(this IRezolverBuilder builder, IEnumerable<IRezolveTarget> targets, Type commonServiceType = null, RezolverPath path = null)
		{
			targets.MustNotBeNull("targets");
			var targetArray = targets.ToArray();
			if (targets.Any(t => t == null))
				throw new ArgumentException("All targets must be non-null", "targets");

			if (path != null)
			{
				if (path.Next == null)
					throw new ArgumentException(Exceptions.PathIsAtEnd, "path");

				//get the named Builder.  If it doesn't exist, create one.
				var childBuilder = builder.GetNamedBuilder(path, true);
				//note here we don't pass the name through.
				//when we support named scopes, we would be lopping off the first item in a hierarchical name to allow for the recursion.
				childBuilder.RegisterMultiple(targets, commonServiceType);
				return;
			}

			//for now I'm going to take the common type from the first target.
			if (commonServiceType == null)
			{
				commonServiceType = targetArray[0].DeclaredType;
			}

			if (targetArray.All(t => t.SupportsType(commonServiceType)))
			{
				IRezolveTargetEntry existing = builder.Fetch(commonServiceType);
				//MultipleRezolveTarget multipleTarget = null;
				//Type targetType = MultipleRezolveTarget.MakeEnumerableType(commonServiceType);

				if (existing != null)
				{
					foreach (var target in targets)
					{
						existing.AddTarget(target);
					}
				}
				else
				{
					foreach (var target in targets)
					{
						builder.Register(target, commonServiceType, null);
					}
				}
			}
			else
				throw new ArgumentException(string.Format(Exceptions.TargetDoesntSupportType_Format, commonServiceType), "target");
		}

		/// <summary>
		/// Batch-registers multiple targets with different contracts.  This is like calling <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/>
		/// multiple times, once for each of the targets in the enumerable.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="targets">The targets to be registered</param>
		public static void RegisterAll(this IRezolverBuilder builder, IEnumerable<IRezolveTarget> targets)
		{
			builder.MustNotBeNull(nameof(builder));

			foreach (var target in targets)
			{
				builder.Register(target);
			}
		}

		/// <summary>
		/// Batch-registers multiple targets with different contracts.  This is like calling <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/>
		/// multiple times, once for each of the targets in the array.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="targets">The targets to be registered.</param>
		public static void RegisterAll(this IRezolverBuilder builder, params IRezolveTarget[] targets)
		{
			RegisterAll(builder, targets.AsEnumerable());
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver via construction.  The registration will auto-bind a constructor (most greedy) on the type
		/// and optionally bind any properties/fields on the new object, depending on the IPropertyBindingBehaviour object passed.
		/// Note that this method supports open generics.
		/// </summary>
		/// <typeparam name="TObject">The type of the object that is to be constructed when resolved.  Also doubles up as the type to be used for the registration itself.</typeparam>
		/// <param name="builder"></param>
		/// <param name="path">See <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/></param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.  If null, then no properties are bound.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
		/// 'Auto' static methods and then registering them.</remarks>
		public static void RegisterType<TObject>(this IRezolverBuilder builder, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			RegisterType(builder, typeof(TObject), path: path, propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// This is the same as <see cref="RegisterType{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>, except the <typeparamref name="TService"/> type
		/// parameter allows you to explicitly set the type against which the registration is to be made.
		/// </summary>
		/// <typeparam name="TObject">See <see cref="RegisterType{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/></typeparam>
		/// <typeparam name="TService">The type against which the registration is to be made in the builder.  E.g. 'IFoo' when TObject is 'Foo'.</typeparam>
		/// <param name="builder"></param>
		/// <param name="path">See <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/></param>
		/// <param name="propertyBindingBehaviour"><see cref="RegisterType{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/></param>
		/// <remarks>Please note the generic parameter constraints on this method: <typeparamref name="TService"/> must have <typeparamref name="TObject"/> as a base or interface
		/// in order for your code to compile.  Note that if you use the <see cref="RegisterType(IRezolverBuilder, Type, Type, RezolverPath, IPropertyBindingBehaviour)"/> overload,
		/// then this is not the case.</remarks>
		public static void RegisterType<TObject, TService>(this IRezolverBuilder builder, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
				where TObject : TService
		{
			RegisterType(builder, typeof(TObject), serviceType: typeof(TService), path: path, propertyBindingBehaviour: propertyBindingBehaviour);
		}

		/// <summary>
		/// Non-generic version of <see cref="RegisterType{TObject, TService}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/>.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="objectType">Required. Type of object to be constructed</param>
		/// <param name="serviceType">See <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/></param>
		/// <param name="path">See <see cref="IRezolverBuilder.Register(IRezolveTarget, Type, RezolverPath)"/></param>
		/// <param name="propertyBindingBehaviour"><see cref="RegisterType{TObject}(IRezolverBuilder, RezolverPath, IPropertyBindingBehaviour)"/></param>
		public static void RegisterType(this IRezolverBuilder builder, Type objectType, Type serviceType = null, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			builder.MustNotBeNull("rezolver");
			objectType.MustNotBeNull("objectType");
			RegisterTypeInternal(builder, objectType, serviceType, path, propertyBindingBehaviour);
		}

		internal static void RegisterTypeInternal(IRezolverBuilder builder, Type objectType, Type serviceType, RezolverPath path, IPropertyBindingBehaviour propertyBindingBehaviour)
		{
			builder.Register(New(objectType, propertyBindingBehaviour), serviceType: serviceType, path: path);
		}

		private static IRezolveTarget New<TObject>(IPropertyBindingBehaviour propertyBindingBehaviour)
		{
			return New(typeof(TObject), propertyBindingBehaviour);
		}

		private static IRezolveTarget New(Type objectType, IPropertyBindingBehaviour propertyBindingBehaviour)
		{
			if (TypeHelpers.IsGenericTypeDefinition(objectType))
				return GenericConstructorTarget.Auto(objectType, propertyBindingBehaviour);
			else
				return ConstructorTarget.Auto(objectType, propertyBindingBehaviour);
		}

		/// <summary>
		/// Generic version of <see cref="RegisterAlias{TAlias, TAliased}(IRezolverBuilder, RezolverPath, RezolverPath)"/>, see that method for more.
		/// </summary>
		/// <typeparam name="TAlias">Type being registered as an alias to another type</typeparam>
		/// <typeparam name="TAliased">The target type of the alias.</typeparam>
		/// <param name="builder"><see cref="RegisterAlias{TAlias, TAliased}(IRezolverBuilder, RezolverPath, RezolverPath)"/></param>
		/// <param name="aliasPath"><see cref="RegisterAlias{TAlias, TAliased}(IRezolverBuilder, RezolverPath, RezolverPath)"/></param>
		/// <param name="aliasedPath"><see cref="RegisterAlias{TAlias, TAliased}(IRezolverBuilder, RezolverPath, RezolverPath)"/></param>
		public static void RegisterAlias<TAlias, TAliased>(this IRezolverBuilder builder, RezolverPath aliasPath = null, RezolverPath aliasedPath = null)
		{
			RegisterAlias(builder, typeof(TAlias), typeof(TAliased), aliasPath, aliasedPath);
		}

		/// <summary>
		/// Registers an alias for one type and path to another type and path.
		/// 
		/// The created entry will effectively represent a second Resolve call into the container for the aliased type.
		/// </summary>
		/// <param name="builder">The builder in which the alias is to be registered</param>
		/// <param name="aliasType">The type to be registered as an alias</param>
		/// <param name="aliasedType">The type being aliased.</param>
		/// <param name="aliasPath">Optional. The path under which the alias will be registered.</param>
		/// <param name="aliasedPath">Optional. The path that is to be used when searching for the aliased target.</param>
		/// <remarks>Use this when it's important that a given target type is always served through the same compiled target, even when the consumer
		/// expects it to be of a different type.  A very common scenario is when you have a singleton instance of TAliased, and need to
		/// serve that same instance for TAlias.  If you register the same singleton for both types, you get two separate singletons for each type.
		/// 
		/// If you register the singleton against TAliased, then register an alias for TAlias, then regardless of which type that is requested, it'll
		/// always be the same instance that is served.</remarks>
		public static void RegisterAlias(this IRezolverBuilder builder, Type aliasType, Type aliasedType, RezolverPath aliasPath = null, RezolverPath aliasedPath = null)
		{
			builder.MustNotBeNull(nameof(builder));
			aliasType.MustNot(t => t == aliasedType, "The aliased type and alias must be different", nameof(aliasType));
			IRezolveTarget target = new RezolvedTarget(aliasedType, aliasedPath != null ? aliasedPath.ToString() : null);
			//if there's no implicit conversion to our alias type from the aliased type, but there is
			//the other way around, then we need to stick in an explicit change of type, otherwise the registration will
			//fail.  This does, unfortunately, give rise to the situation where we could be performing an invalid cast - but that
			//will come out in the wash at runtime.
			if (!TypeHelpers.IsAssignableFrom(aliasType, aliasedType) &&
				TypeHelpers.IsAssignableFrom(aliasedType, aliasType))
				target = new ChangeTypeTarget(target, aliasType);

			builder.Register(target, aliasType, aliasPath);
		}
  }
}
