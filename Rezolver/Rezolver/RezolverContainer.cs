using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// </summary>
	public class RezolverContainer : IRezolverContainer
	{
		#region nested cach stuff

		public RezolverCache RezolverCache
		{
			get { return _rezolverCache; }
		}

		//TODO: implement a second cache for entries with no name.
		#endregion

		private readonly IRezolverScope _scope;
		private readonly RezolverCache _rezolverCache;

		public RezolverContainer(IRezolverScope scope)
		{
			//TODO: check for null scope.
			_scope = scope;
			_rezolverCache = new RezolverCache(this, new RezolverTargetCompiler());
		}

		public bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return _scope.Fetch(type, name) != null;
			//return RezolverCache.GetFactory(type, CreateFactoryFunc, name) != null;
		}

		public bool CanResolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: And again - change this to refer to the cache
			return _scope.Fetch<T>(name) != null;
		}

		public object Resolve(Type type, IRezolverContainer dynamicContainer, string name = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve(type, name))
					return dynamicContainer.Resolve(type, name);

				var factory = RezolverCache.GetDynamicFactory(type, (t, n) => _scope.Fetch(t, n), name);
				if (factory == null)
					throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format("with name \"{0}\"", name) : ""));

				return factory(dynamicContainer);
			}
			else
			{
				var factory = RezolverCache.GetStaticFactory(type, (t, n) => _scope.Fetch(t, n), name);
				if (factory == null)
					throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format("with name \"{0}\"", name) : ""));

				return factory();
			}
		}

		public object Resolve(Type type, string name = null)
		{
			var factory = RezolverCache.GetStaticFactory(type, (t, n) => _scope.Fetch(t, n), name);
			if (factory == null)
				throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format("with name \"{0}\"", name) : ""));

			return factory();
		}

		public T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve<T>(name))
					return dynamicContainer.Resolve<T>(name);

				var factory = RezolverCache.GetDynamicFactory<T>((t, n) => _scope.Fetch(t, n), name);
				if (factory == null)
					throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", typeof(T), name != null ? string.Format("with name \"{0}\"", name) : ""));

				return factory(dynamicContainer);
			}
			else
			{
				var factory = RezolverCache.GetStaticFactory<T>((t, n) => _scope.Fetch(t, n), name);
				if (factory == null)
					throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", typeof(T), name != null ? string.Format("with name \"{0}\"", name) : ""));

				return factory();
			}
		}

		private Func<IRezolverContainer, object> CreateFactoryFunc(Type type, string name)
		{

			var target = _scope.Fetch(type, name);
			if (target != null)
			{
				//slight issue here - if the target expression returns value type, then we're forcing a box/unbox
				//on this delegate and when it's called for the caller.
				//this perhaps could be fixed with a generic version of Resolve

				//notice that despite the dynamic container being passed to this method, it's not the scope
				//that's provided to the CreateExpression call.  Instead, passing that container is deferred
				//until we execute the the delegate itself.  To support such dynamic scoping, a target must simply
				//reference the parameter expression ExpressionHelper.DynamicContainerParam.
				return ExpressionHelper.GetLambdaForTarget(this, type, target).Compile();
			}
			return null;

		}

		public void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			//you are not allowed to register targets directly into a container
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			return _scope.Fetch(type, name);
		}

		public IRezolveTarget Fetch<T>(string name = null)
		{
			return _scope.Fetch(typeof(T), name);
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scopee, wwe don't support the call.
			if (create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}


	}
}