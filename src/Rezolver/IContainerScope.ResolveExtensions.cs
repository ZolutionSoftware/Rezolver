using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for <see cref="IContainerScope"/> so that instances of that
	/// interface present a similar set of functionality to <see cref="IContainer"/>.
	/// </summary>
	public static class ContainerScopeResolveExtensions
	{
		/// <summary>
		/// Resolves an object through the scope's <see cref="IContainerScope.Container"/>
		/// </summary>
		/// <typeparam name="TResult">The type of object required.</typeparam>
		/// <param name="scope">The active scope within which the call is to be made.</param>
		/// <returns></returns>
		/// <remarks>Resolving an object via a scope does not guarantee that it will be
		/// tracked.  Ultimately, it's up to the behaviour of the individual underlying
		/// targets to determine whether they should interact with the scope.
		/// 
		/// Indeed, all this extension method does is to forward the method call on to the
		/// <see cref="IContainerScope.Container"/> of the given scope, ensuring that
		/// the scope is set on the <see cref="ResolveContext"/> that is passed to its
		/// <see cref="IContainer.Resolve(ResolveContext)"/> method.
		/// </remarks>
		public static TResult Resolve<TResult>(this IContainerScope scope)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			return (TResult)scope.Container.Resolve(new ResolveContext(scope, typeof(TResult)));
		}

		/// <summary>
		/// Non-generic variant of the <see cref="Resolve{TResult}(IContainerScope)"/> 
		/// extension method.
		/// </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="requestedType">Type of object required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static object Resolve(this IContainerScope scope, Type requestedType)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			return scope.Container.Resolve(new ResolveContext(scope, requestedType));
		}
	}
}
