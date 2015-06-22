using System;
using System.Collections.Generic;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// A rezolver that's also a lifetime scope - that is, it's disposable,
	/// and will dispose of any disposable instances that it creates when it's disposed.
	/// 
	/// Also, any subsequent lifetime scopes that it, or any child, creates will 
	/// be disposed of when this scope is disposed.
	/// 
	/// Note that while a lifetime scope can track objects of any types, it only *automatically*
	/// tracks disposable objects.  To force a scope to track an instance, regardless of whether it's
	/// dispoable or not, you can call <see cref="AddToScope"/>.
	/// 
	/// This is how the default ScopedSingletonTarget works - if an object with this lifetime isn't a 
    /// disposable, it is explicitly added to the scope passed to it at runtime, and then when an instance
    /// is subsequently requested, the code compiled by the scoped singleton will search the current scope,
    /// for an existing instance, before creating one.
	/// </summary>
	public interface ILifetimeScopeRezolver : IRezolver, IDisposable
	{
		/// <summary>
		/// If this lifetime scope is a child of another, this will be non-null.
		/// </summary>
		ILifetimeScopeRezolver ParentScope { get; }
		/// <summary>
		/// Registers an instance to this scope which, if disposable, will then be disposed 
		/// when this scope is disposed.
		/// </summary>
		/// <param name="obj">The object; if null, then no operation is performed.  Doesn't have to be IDisposable, but if it is, 
		/// then it will be tracked for disposal.</param>
		/// <param name="context">Optional - a rezolve context representing the conditions under which 
		/// the object should be returned in the enumerable returned from a call to GetFromScope</param>
		void AddToScope(object obj, RezolveContext context = null);

		/// <summary>
		/// Retrieves all objects from this scope that were previously added through a call to 
		/// <see cref="AddToScope" /> with RezolveContexts that match the one passed.
		/// 
		/// The method never returns null.
		/// </summary>
		/// <param name="context">Required - the context whose properties will be used to find matching
		/// objects.</param>
		/// <returns></returns>
		IEnumerable<object> GetFromScope(RezolveContext context);		
	}

	public static class ILifetimeScopeRezolverExtensions
	{
		/// <summary>
		/// Retrieves a single instance that was previously added to the scope (or,
		/// optionally parent scopes) through a call to <see cref="ILifetimeScopeRezolver.AddToScope"/> 
		/// with a RezolveContext matching the one passed.
		/// 
		/// Note - if multiple matches are found in a single scope, an InvalidOperationException will be thrown.
		/// </summary>
		/// <param name="scope">Required.  The scope to be searched and optionally whose parent scopes
		/// are to be searched.</param>
		/// <param name="context">Required.  The context whose properties will be used to find the
		/// matching disposable.</param>
		/// <param name="searchAncestors">Pass true to continue searching all parent scopes until one or 
		/// more matches are found.  In this mode, an InvalidOperationException is thrown if any of the parent
		/// scopes contain more than one match.</param>
		/// <returns>The matched object or null if one is not found.</returns>
		public static object GetSingleFromScope(this ILifetimeScopeRezolver scope, RezolveContext context, bool searchAncestors = false)
		{
			scope.MustNotBeNull("scope");
			context.MustNotBeNull("context");
			//this method is written in such a way as to avoid realising the enumerable to get the count
			//of items found.
			object result = null;
			if(searchAncestors)
			{
				var current = scope;
				while (current != null)
				{
					var enumerable = current.GetFromScope(context);
					var enumerator = enumerable.GetEnumerator();

					if(!enumerator.MoveNext())
					{
						current = current.ParentScope;
					}
					else
					{
						result = enumerator.Current;
						if (enumerator.MoveNext())
							throw new InvalidOperationException(Exceptions.MoreThanOneObjectFoundInScope);
						break;
					}
				}
			}
			else
			{
				var enumerable = scope.GetFromScope(context);
				var enumerator = enumerable.GetEnumerator();

				if (enumerator.MoveNext())
				{
					result = enumerator.Current;
					if (enumerator.MoveNext())
						throw new InvalidOperationException(Exceptions.MoreThanOneObjectFoundInScope);
				}
			}

			return result;
		}

        /// <summary>
        /// As the name suggests - this grabs the root scope for the scope on which this method is invoked.
        /// </summary>
        /// <param name="scope">The scope whose root scope is sought.  If null, then the method also returns null.</param>
        /// <returns>An <see cref="ILifetimeScopeRezolver"/> reference representing the top-level (root) scope
        /// for the passed scope.</returns>
        public static ILifetimeScopeRezolver GetScopeRoot(this ILifetimeScopeRezolver scope)
        {
            if (scope == null)
                return null;
            
            while(scope.ParentScope != null)
            {
                scope = scope.ParentScope;
            }

            return scope;
        }

        /// <summary>
        /// This is primarily used by the dynamic code generated by RezolveTargetBase - if the scope is non-null
        /// and the instance is an IDiposable, then the object will be added to the scope.
        /// 
        /// The <paramref name="instance"/>, either way, is returned to simplify the function's use in dynamic code 
        /// produced from expression trees.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T TrackIfScopedAndDisposableAndReturn<T>(this ILifetimeScopeRezolver scope, T instance)
        {
            if(scope != null && instance is IDisposable)
                scope.AddToScope(instance);

            return instance;
        }
	}
}