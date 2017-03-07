using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Describes different ways in which objeects interact with scopes.
	/// </summary>
	/// <remarks>Note: this enum *might* be replaced with an abstraction in the future.  If so,
	/// it will not alter how regatrations are performed, but it will affect any low-level code
	/// which uses this enum directly.</remarks>
	public enum ScopeBehaviour
	{
		/// <summary>
		/// Implicitly scoped objects are only added to the scope 
		/// for the purposes of disposing when the scope is disposed
		/// </summary>
		Implicit = 0,
		/// <summary>
		/// Explicitly scoped objects act like singletons in the current scope, regardless of
		/// whether they are disposable or not.
		/// </summary>
		Explicit = 1,
		/// <summary>
		/// The object will not be tracked in any scope, regardless of whether there is one active,
		/// or whether the object is disposable.
		/// </summary>
		None
	}

	/// <summary>
	/// This is an <see cref="IContainer" />-like object (resolving functionality is provided through the extension methods
	/// in the <see cref="ContainerScopeResolveExtensions" /> class) which provides lifetime scoping for disposable objects,
	/// and scoped singleton functionality for any object.
	/// 
	/// Implementations of this interface must, in their implementation of <see cref="IScopeFactory"/>, create a child scope
	/// of this scope.
	/// </summary>
	/// <seealso cref="Rezolver.IScopeFactory" />
	/// <seealso cref="System.IDisposable" />
	/// <seealso cref="System.IServiceProvider" />
	public interface IContainerScope : IDisposable, IServiceProvider, IScopeFactory
	{
		/// <summary>
		/// If this scope has a parent scope, this is it.
		/// </summary>
		IContainerScope Parent { get; }
		/// <summary>
		/// The container that this scope is tied to.  All standard resolve operations
		/// should be made against this container to begin with.
		/// </summary>
		IContainer Container { get; }
		/// <summary>
		/// Called by child scopes when they are disposed to notify the parent that they 
		/// will no longer need to be disposed of when the parent is disposed.
		/// </summary>
		/// <param name="child"></param>
		/// <remarks>This is an infrastructure method and not something you would usually need to call.
		/// It's exposed for developers who are extending the container scoping functionality only.</remarks>
		void ChildScopeDisposed(IContainerScope child);
		/// <summary>
		/// Execute the given object factory within this scope.  Depending on the
		/// scoping behaviour passed, the object will either be resolved directly from the
		/// scope (i.e. existing objects contained within it) or obtained by executing
		/// the factory and optionally tracking the object if it's IDisposable.
		/// </summary>
		/// <param name="context">The resolve context - please note that the container
		/// that's present on this is the actual container that should be used to 
		/// resolve objects</param>
		/// <param name="factory">The factory to be executed</param>
		/// <param name="behaviour"></param>
		/// <returns></returns>
		/// <remarks>This function is the primary workhorse of all scopes.  Most importantly,
		/// the object produced from the factory DOES NOT have to come from this scope's
		/// <see cref="Container"/> - the implementing type simply has to ensure that 
		/// it tracks whatever object is ultimately returned; potentially returning
		/// a previously tracked object if <paramref name="behaviour"/> is 
		/// <see cref="ScopeBehaviour.Explicit"/></remarks>
		object Resolve(ResolveContext context, Func<ResolveContext, object> factory, ScopeBehaviour behaviour);
		//REVIEW: The enum solution for this method works fine for now, but offers no scope for extending it outside of the Rezolver codebase.
		//The more extensible solution would be to have an interface which represents the behaviour so that the logic for that behaviour can be abstracted away
		//The difficulty with this being that it means the underlying storage containers for scoped objects used by the scope needs to exposed to implementations
		//of that interface.  The current implementation, for example, uses privately declared concurrent dictionaries and a synchronised list to track objects - this
		//storage would need to be abstracted away.
	}
}
