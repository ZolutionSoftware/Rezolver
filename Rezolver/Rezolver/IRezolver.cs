using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// The primary IOC entry point for the Rezolver framework.  This interface implies more than just
	/// an object that can resolve dependencies or locate services - for example, it is suggested that an <see cref="IRezolverBuilder"/> instance
	/// (see <see cref="Builder"/>) be used to store the core type registrations from which this resolver will be built.
	/// </summary>
	/// <remarks>
	/// If an implementation is indeed using the <see cref="Builder"/> to build a set of registrations from which objects will be created, 
	/// then that implementationi should, also, allow for new registrations to be added to the builder throughout the lifetime of the 
	/// <see cref="IRezolver"/>.
	/// 
	/// However - A caller cannot, expect to be able to resolve Type 'X',
	/// then make some modification to the builder which can causes Type 'X' to be built differently, the next time it is resolved.  Implementations
	/// of <see cref="IRezolver"/> are free to treat the a dependency graph of an object of a resolved type (not necessarily the objects themselves,
	/// but the types of objects that are resolved and how they, in turn, are built) as fixed after the first resolve operation is done.
	/// </remarks>
	public interface IRezolver : IServiceProvider
	{
		/// <summary>
		/// Provides access to the builder for this rezolver - so that registrations can be added to the rezolver after
		/// construction.  It is not a requirement of a rezolver to use a builder to act as source of registrations, therefore 
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
		/// If you're going to be calling <see cref="Resolve"/> immediately afterwards, consider using the TryResolve method instead,
		/// which allows you to check and obtain the result at the same time.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns><c>true</c> if this instance can resolve the specified context; otherwise, <c>false</c>.</returns>
		bool CanResolve(RezolveContext context);

		/// <summary>
		/// The core 'resolve' operation in Rezolver.
		/// 
		/// The object is resolved using state from the passed <paramref name="context"/> (type to be resolved, any names,
		/// lifetime scopes, and a reference to the original resolver instance that is 'in scope', which could be a different resolver to
		/// this resolver.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The resolved object, if successful, otherwise an exception should be raised.</returns>
		object Resolve(RezolveContext context);

		/// <summary>
		/// Merges the CanResolve and Resolve operations into one call.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="result">The result.</param>
		/// <returns><c>true</c> if the operation succeeded (the resolved object will be set into the <paramref name="result"/> 
		/// parameter); <c>false</c> otherwise.</returns>
		bool TryResolve(RezolveContext context, out object result);

		/// <summary>
		/// Called to create a lifetime scope that will track, and dispose of, any 
		/// disposable objects that are created via calls to <see cref="Resolve"/> (to the lifetime scope
		/// itself, not to the resolver that 'parents' the lifetime scope).
		/// </summary>
		/// <returns>An <see cref="ILifetimeScopeRezolver"/> instance that will use this resolver to resolve objects,
		/// but which will impose its own lifetime restrictions on those instances.</returns>
		ILifetimeScopeRezolver CreateLifetimeScope();


		/// <summary>
		/// Fetches the compiled target for the given context.
		/// 
		/// This is not typically a method that consumers of an <see cref="IRezolver" /> are likely to use; it's more typically
		/// used by code generation code (or even generated code) to interoperate between two resolvers, or indeed over other object.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>ICompiledRezolveTarget.</returns>
		ICompiledRezolveTarget FetchCompiled(RezolveContext context);
	}

	//note - I've gone the overload route instead of default parameters to aid runtime optimisation
	//in the case of one or two parameters only being required.  Believe me, it does make a difference.

	/// <summary>
	/// Extension methods to simplify calls to <see cref="IRezolver.Resolve" />, which hide the creation of
	/// a <see cref="RezolveContext" />; and other methods.
	/// </summary>
	public static class IRezolverExtensions
	{
		/// <summary>
		/// Resolves an object of the given <paramref name="type"/>
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <returns>An instance of the <paramref name="type"/>.</returns>
		public static object Resolve(this IRezolver rezolver, Type type)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type));
		}

		/// <summary>
		/// Resolves an object of the given <paramref name="type"/>, attempting to use a named registration
		/// in the <paramref name="rezolver"/> with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <returns>An instance of the requested <paramref name="type"/>.</returns>
		public static object Resolve(this IRezolver rezolver, Type type, string name)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type, name));
		}

		/// <summary>
		/// Resolves an object of the given <paramref name="type" />, using the
		/// given lifetime <paramref name="scope" /> for lifetime management.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">Type to be resolved.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <returns>An instance of the requested <paramref name="type"/>.</returns>
		public static object Resolve(this IRezolver rezolver, Type type, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type, scope));
		}

		/// <summary>
		/// Resolves an object of the given <paramref name="type"/>, attempting to use a named registration
		/// in the <paramref name="rezolver"/> with the given <paramref name="name"/>, using the the given 
		/// lifetime <paramref name="scope"/> for lifetime management.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">Type to be resolved.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <returns>An instance of the requested <paramref name="type"/>.</returns>
		public static object Resolve(this IRezolver rezolver, Type type, string name, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type, name, scope));
		}

		/// <summary>
		/// Resolves an object of type <typeparamref name="TObject"/> 
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <returns>An instance of <typeparamref name="TObject"/>.</returns>
		public static TObject Resolve<TObject>(this IRezolver rezolver)
		{
			return (TObject)rezolver.Resolve(typeof(TObject));
		}

		/// <summary>
		/// Resolves an object of the type <typeparamref name="TObject"/>, attempting to use a named registration
		/// in the <paramref name="rezolver"/> with the given <paramref name="name"/>.
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <returns>An instance of <typeparamref name="TObject"/></returns>
		public static TObject Resolve<TObject>(this IRezolver rezolver, string name)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), name);
		}

		/// <summary>
		/// Resolves an object of the type <typeparamref name="TObject" />, using the
		/// given lifetime <paramref name="scope" /> for lifetime management.
		/// </summary>
		/// <typeparam name="TObject">Type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <returns>An instance of <typeparamref name="TObject"/>.</returns>
		public static TObject Resolve<TObject>(this IRezolver rezolver, ILifetimeScopeRezolver scope)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), scope);
		}

		/// <summary>
		/// Resolves an object of the type <typeparamref name="TObject"/>, attempting to use a named registration
		/// in the <paramref name="rezolver"/> with the given <paramref name="name"/>, using the the given 
		/// lifetime <paramref name="scope"/> for lifetime management.
		/// </summary>
		/// <typeparam name="TObject">Type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <returns>An instance of <typeparamref name="TObject"/>.</returns>
		public static TObject Resolve<TObject>(this IRezolver rezolver, string name, ILifetimeScopeRezolver scope)
		{
			return (TObject)rezolver.Resolve(typeof(TObject), name, scope);
		}

		/// <summary>
		/// The same as the Resolve method with the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
		/// <remarks>For more detail on the <paramref name="type"/> parameter, see one of the non-generic <see cref="Resolve"/> 
		/// overloads</remarks>
		public static bool TryResolve(this IRezolver rezolver, Type type, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type), out result);
		}

		/// <summary>
		/// The same as the Resolve method with the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
		/// <remarks>For more detail on the <paramref name="type"/> parameter, see one of the non-generic <see cref="Resolve"/> 
		/// overloads</remarks>
		public static bool TryResolve(this IRezolver rezolver, Type type, string name, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name), out result);
		}

		/// <summary>
		/// The same as the Resolve method with the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
		/// <remarks>For more detail on the <paramref name="type"/> parameter, see one of the non-generic <see cref="Resolve"/> 
		/// overloads</remarks>
		public static bool TryResolve(this IRezolver rezolver, Type type, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, scope), out result);
		}

		/// <summary>
		/// The same as the Resolve method with the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="type">The type to be resolved.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
		/// <remarks>For more detail on the <paramref name="type"/> parameter, see one of the non-generic <see cref="Resolve"/> 
		/// overloads</remarks>
		public static bool TryResolve(this IRezolver rezolver, Type type, string name, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name, scope), out result);
		}

		/// <summary>
		/// The same as the generic Resolve method the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
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

		/// <summary>
		/// The same as the generic Resolve method the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
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

		/// <summary>
		/// The same as the generic Resolve method the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
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

		/// <summary>
		/// The same as the generic Resolve method the same core parameter types, except this will not throw
		/// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
		/// returning the created object (if successful) in the <paramref name="result"/> parameter.
		/// </summary>
		/// <typeparam name="TObject">The type to be resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="name">The name to be used in searching for a registration within the resolver.  Note
		/// that the standard behaviour is that if a registration can't be found matching the name, then the 
		/// search is re-attempted without the name.</param>
		/// <param name="scope">The lifetime scope - can be a different instance to <paramref name="rezolver"/>
		/// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
		/// master rezolver instance is used to create a child 'scope' which has a different lifetime to that
		/// of the master.</param>
		/// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
		/// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
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
		/// Directly registers a target, optionally for a particular target type and optionally
		/// under a particular name.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="target">Required.  The target to be registered</param>
		/// <param name="type">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="IRezolveTarget.DeclaredType"/> on the <paramref name="target" /></param>
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
		/// existing registration(s) or extending them.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="targets">The targets to be registered.</param>
		/// <param name="commonServiceType">Optional - The type shared between each of the targets.</param>
		/// <param name="path">Optional - The path under which the registration is to be made.</param>
		/// <param name="append">if set to <c>true</c> then any existing registration(s) is/are to be kept.</param>
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
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="type">Optional - the type the registration is to be made against, if different from <typeparamref name="T"/>.</param>
		/// <param name="path">Optional - The path under which the registration is to be made.</param>
		/// <param name="adapter">Optional - an adapter that is capable of translating the expressions in the tree represented by <paramref name="expression"/> into a target.</param>
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
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="obj">The object.</param>
		/// <param name="type">The type.</param>
		/// <param name="path">The path.</param>
		/// <remarks>This is equivalent to creating an <see cref="ObjectTarget"/> and registering it.</remarks>
		public static void RegisterObject<T>(this IRezolver rezolver, T obj, Type type = null, RezolverPath path = null)
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);
			rezolver.Builder.Register(obj.AsObjectTarget(type), type, path);
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver via construction.  The registration will auto-bind a constructor (most greedy) on the type
		/// and optionally bind any properties/fields on the new object, dependding on the IPropertyBindingBehaviour object passed.
		/// Note that this method supports open generics.
		/// </summary>
		/// <typeparam name="TObject">The type of the object that is to be constructed when resolved.</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="path">The path.</param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
		/// 'Auto' static methods and then registering them.</remarks>
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
		/// Registers a type to be created by the Rezolver when a particular service type is requested.  The registration will auto-bind a
		/// constructor (most greedy) on the type and optionally bind any properties/fields on the new object, depending on the
		/// IPropertyBindingBehaviour object passed.
		/// Note that this method supports open generics.
		/// </summary>
		/// <typeparam name="TObject">The type of the object that is to be constructed when resolved.</typeparam>
		/// <typeparam name="TService">The type that is to be registered in the resolver's builder..</typeparam>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="path">The path.</param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
		/// 'Auto' static methods and then registering them.</remarks>
		public static void RegisterType<TObject, TService>(this IRezolver rezolver, RezolverPath path = null, IPropertyBindingBehaviour propertyBindingBehaviour = null)
			where TObject : TService
		{
			rezolver.MustNotBeNull("rezolver");
			RezolverMustHaveBuilder(rezolver);

			if (TypeHelpers.IsGenericTypeDefinition(typeof(TObject)))
				rezolver.Builder.Register(GenericConstructorTarget.Auto<TObject>(propertyBindingBehaviour), type: typeof(TService), path: path);
			else
				rezolver.Builder.Register(ConstructorTarget.Auto<TObject>(propertyBindingBehaviour), type: typeof(TService), path: path);
		}

		/// <summary>
		/// Registers a type to be created by the Rezolver when it is requested.  The registration will auto-bind a 
		/// constructor (most greedy) on the type and optionally bind any properties/fields on the new object, depending on the 
		/// IPropertyBindingBehaviour object passed.
		/// 
		/// Note that this method supports open generics.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="objectType">Type of the object to be created and registered.</param>
		/// <param name="path">The path.</param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
		/// 'Auto' static methods and then registering them.</remarks>
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
		/// Note that this method supports open generics.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="objectType">Type of the object that is to be construced when resolved.</param>
		/// <param name="serviceType">Type for which the object is to be created.</param>
		/// <param name="path">The path.</param>
		/// <param name="propertyBindingBehaviour">The property binding behaviour.</param>
		/// <remarks>This is equivalent to creating either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> via their
		/// 'Auto' static methods and then registering them.</remarks>
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

		/// <summary>
		/// Parameter array version of the RegisterAll method.
		/// </summary>
		/// <param name="rezolver">The rezolver.</param>
		/// <param name="targets">The targets.</param>
		public static void RegisterAll(this IRezolver rezolver, params IRezolveTarget[] targets)
		{
			RegisterAll(rezolver, targets.AsEnumerable());
		}
	}
}