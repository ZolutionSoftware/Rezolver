using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// The final compiled rezolver built from an IRezolverBuilder
	/// Note that the interface also implements the IRezolverBuilder interface in order that it
	/// can provide both concrete instances and the means by which to build them, in order to 
	/// allow separate rezolvers to chain.  Rezolvers are not expected to implement IRezolverBuilder directly,
	/// however, they are expected to proxy the inner <see cref="Builder"/> - however consumers looking to 
	/// get registrations of a rezolver should always do it through the rezolver, in case of any special logic
	/// being applied outside of the builder itself.
	/// </summary>
	public interface IRezolver : IServiceProvider
	{
		/// <summary>
		/// Provides access to the builder for this rezolver - so that registrations can be added to the rezolver after
		/// conostruction.  It is not a requirement of a rezolver to use a builder to act as source of registrations, therefore 
		/// if a builder is not applicable to this instance, either return a stub instance that always returns notargets, or
		/// throw a NotSupportException.
		/// </summary>
		IRezolverBuilder Builder { get; }
		/// <summary>
		/// Provides access to the compiler used by this rezolver in turning IRezolveTargets into
		/// ICompiledRezolveTargets.
		/// </summary>
		IRezolveTargetCompiler Compiler { get; }
		/// <summary>
		/// Returns true if a resolve operation for the given context will succeed.
		/// 
		/// If you're going to be calling resoolve immediately afterwards, consider using the TryResolve method instead, 
		/// which allows you to check and obtain the result at the same time.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="dynamic"></param>
		/// <returns></returns>
		bool CanResolve(RezolveContext context);

		/// <summary>
		/// Resolves an object of the given type, optionally with the given name, using the optional
		/// dynamic rezolver for any late-bound resolve calls.
		/// </summary>
		/// <param name="type">Required. The type of the dependency to be resolved.</param>
		/// <param name="name">Optional.  The name of the dependency to be resolved.</param>
		/// <param name="dynamicRezolver">Optional.  If provided, then the eventual rezolved
		/// instance could either come from, or have one or more of it's dependencies come from,
		/// here instead of from this rezolver.</param>
		/// <returns></returns>
		object Resolve(RezolveContext context);

		/// <summary>
		/// Merges the CanResolve and Resolve operations into one call.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		bool TryResolve(RezolveContext context, out object result);

		/// <summary>
		/// Called to create a lifetime scope that will track, and dispose of, any 
		/// disposable objects that are created.
		/// </summary>
		/// <returns></returns>
		ILifetimeScopeRezolver CreateLifetimeScope();

		//TODO: I think we need this in the IRezolver interface so that we can more explictly share
		//compiled code between rezolvers.
		ICompiledRezolveTarget FetchCompiled(RezolveContext context);
	}

	//note - I've gone the overload route instead of default parameters to aid runtime optimisation
	//in the case of one or two parameters only being required.  Believe me, it does make a difference.

	public static class IRezolverExtensions
	{
		public static object Resolve(this IRezolver rezolver, Type type)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type));
		}

		public static object Resolve(this IRezolver rezolver, Type type, string name)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type, name));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, requestedType, scope));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, requestedType, name, scope));
		}

		public static TObject Resolve<TObject>(this IRezolver rezolver)
		{
			return (TObject)rezolver.Resolve(typeof(TObject));
		}

		public static TObject Resolve<TObject>(this IRezolver rezolver, string name)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), name);
		}

		public static TObject Resolve<TObject>(this IRezolver rezolver, ILifetimeScopeRezolver scope)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), scope);
		}

		public static TObject Resolve<TObject>(this IRezolver rezolver, string name, ILifetimeScopeRezolver scope)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), name, scope);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, string name, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, scope), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, string name, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name, scope), out result);
		}

		public static bool TryResolve<TObject>(this IRezolver rezolver, out TObject result)
		{
			object oResult;
			var success = rezolver.TryResolve(out oResult);
			if (success)
				result = (TObject)oResult;
			else
				result = default(TObject);
			return success;
		}

		public static bool TryResolve<TObject>(this IRezolver rezolver, string name, out TObject result)
		{
			object oResult;
			var success = rezolver.TryResolve(name, out oResult);
			if (success)
				result = (TObject)oResult;
			else
				result = default(TObject);
			return success;
		}

		public static bool TryResolve<TObject>(this IRezolver rezolver, ILifetimeScopeRezolver scope, out TObject result)
		{
			object oResult;
			var success = rezolver.TryResolve(scope, out oResult);
			if (success)
				result = (TObject)oResult;
			else
				result = default(TObject);
			return success;
		}

		public static bool TryResolve<TObject>(this IRezolver rezolver, string name, ILifetimeScopeRezolver scope, out TObject result)
		{
			object oResult;
			var success = rezolver.TryResolve(name, scope, out oResult);
			if (success)
				result = (TObject)oResult;
			else
				result = default(TObject);
			return success;
		}


		//registration extensions - wrap around the resolver's Builder object

		/// <summary>
		/// Method which validates that a rezolver can take registrations through a builder.
		/// </summary>
		/// <param name="rezolver"></param>
		/// <param name="parameterName"></param>
		private static void RezolverMustHaveBuilder(IRezolver rezolver, string parameterName = "rezolver")
		{
			try
			{
				if (rezolver.Builder == null)
					throw new ArgumentException("rezolver's Builder property must be non-null", parameterName);
			}
			catch (NotSupportedException ex)
			{
				throw new ArgumentException("rezolver does not support registration through its IRezolverBuilder", ex);
			}
		}

		/// <summary>
		/// Registers a target, optionally for a particular target type and optionally
		/// under a particular name.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="type">Optional.  The type thee target is to be registered against, if different
		/// from the declared type on the <paramref name="target"/></param>
		/// <param name="path">Optional.  The path under which this target is to be registered.  One or more
		/// new named rezolvers could be created to accommodate the registration.</param>
		public static void Register(this IRezolver rezolver, IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);
			rezolver.Builder.Register(target, type, path);
		}


		/// <summary>
		/// Called to register multiple rezolve targets against a shared contract, optionally replacing any 
		/// existing registration(s) or extending them.  In the case of a builder that is a child of another, 
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="commonServiceType"></param>
		/// <param name="path"></param>
		/// <param name="append"></param>
		public static void RegisterMultiple(this IRezolver rezolver, IEnumerable<IRezolveTarget> targets, Type commonServiceType = null, RezolverPath path = null, bool append = true)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);
			rezolver.Builder.RegisterMultiple(targets, commonServiceType, path, append);
		}

		/// <summary>
		/// Called to register a generic expression to be used to produce an instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rezolver"></param>
		/// <param name="expression"></param>
		/// <param name="type"></param>
		/// <param name="path"></param>
		/// <param name="adapter"></param>
		public static void RegisterExpression<T>(this IRezolver rezolver, Expression<Func<RezolveContextExpressionHelper, T>> expression, Type type = null, RezolverPath path = null, IRezolveTargetAdapter adapter = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);
			rezolver.Builder.Register<T>(expression, type, path, adapter);
		}

		//now for the fancy extension methods that shortcut the targets - note these probably all need to be duplicated for IRezolverBuilder too

		/// <summary>
		/// Registers an instance of an object to be used for a particular type.  If you don't pass a type, then 
		/// the type of the object as given by the type parameter T is used.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rezolver"></param>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="path"></param>
		public static void RegisterObject<T>(this IRezolver rezolver, T obj, Type type = null, RezolverPath path = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);
			rezolver.Builder.Register(obj.AsObjectTarget(type), type, path);
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver.  The registration will auto-bind a constructor (most greedy) on the type
		/// and optionally bind any properties/fields on the new object, dependding on the IPropertyBindingBehaviour object passed.
		/// 
		/// Note that this method supports open generics.
		/// </summary>
		/// <typeparam name="TObject"></typeparam>
		/// <param name="rezolver"></param>
		/// <param name="path"></param>
		/// <param name="propertyBindingBehaviour"></param>
		public static void RegisterType<TObject>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);

			if (TypeHelpers.IsGenericTypeDefinition(typeof(TObject)))
				rezolver.Builder.Register(GenericConstructorTarget.Auto<TObject>(propertyBindingBehaviour), path: path);
			else
				rezolver.Builder.Register(ConstructorTarget.Auto<TObject>(propertyBindingBehaviour), path: path);
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver when a particular service type is request.  The registration will auto-bind a 
		/// constructor (most greedy) on the type and optionally bind any properties/fields on the new object, dependding on the 
		/// IPropertyBindingBehaviour object passed.
		/// 
		/// Note that this method supports open generics.
		/// </summary>
		/// <typeparam name="TObject"></typeparam>
		/// <typeparam name="TService"></typeparam>
		/// <param name="rezolver"></param>
		/// <param name="path"></param>
		/// <param name="propertyBindingBehaviour"></param>
		public static void RegisterType<TObject, TService>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);

			if (TypeHelpers.IsGenericTypeDefinition(typeof(TObject)))
				rezolver.Builder.Register(GenericConstructorTarget.Auto<TObject>(propertyBindingBehaviour), type: typeof(TService), path: path);
			else
				rezolver.Builder.Register(ConstructorTarget.Auto<TObject>(propertyBindingBehaviour), type: typeof(TService), path: path);
		}

		public static void RegisterType(this IRezolver rezolver, Type objectType, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			rezolver.MustNotBeNull("rezolver");
			objectType.MustNotBeNull("objectType");
			RezolverMustHaveBuilder(rezolver);

			if (TypeHelpers.IsGenericTypeDefinition(objectType))
				rezolver.Builder.Register(GenericConstructorTarget.Auto(objectType, propertyBindingBehaviour), path: path);
			else
				rezolver.Builder.Register(ConstructorTarget.Auto(objectType, propertyBindingBehaviour), path: path);
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver when a particular service type is request.  The registration will auto-bind a 
		/// constructor (most greedy) on the type and optionally bind any properties/fields on the new object, dependding on the 
		/// IPropertyBindingBehaviour object passed.
		/// 
		/// Note that this method supports open generics.
		/// </summary>
		/// <param name="rezolver"></param>
		/// <param name="path"></param>
		/// <param name="propertyBindingBehaviour"></param>
		public static void RegisterType(this IRezolver rezolver, Type objectType, Type serviceType, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
		{
			rezolver.MustNotBeNull("rezolver");
			objectType.MustNotBeNull("objectType");
			serviceType.MustNotBeNull("serviceType");
			RezolverMustHaveBuilder(rezolver);

			if (TypeHelpers.IsGenericTypeDefinition(objectType))
				rezolver.Builder.Register(GenericConstructorTarget.Auto(objectType, propertyBindingBehaviour), type: serviceType, path: path);
			else
				rezolver.Builder.Register(ConstructorTarget.Auto(objectType, propertyBindingBehaviour), type: serviceType, path: path);
		}

		/// <summary>
		/// Batch-registers multiple targets with different contracts.  Basically, this is like calling Register(IRezolveTarget) multiple times, but with an 
		/// enumerable.
		/// </summary>
		/// <param name="rezolver"></param>
		/// <param name="targets"></param>
		public static void RegisterAll(this IRezolver rezolver, IEnumerable<IRezolveTarget> targets)
		{
			foreach (var target in targets)
			{
				rezolver.Register(target);
			}
		}
	}
}