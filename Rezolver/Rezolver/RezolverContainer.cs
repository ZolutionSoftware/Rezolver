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
			_rezolverCache = new RezolverCache();
		}

		public bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			return RezolverCache.GetFactory(type, CreateFactoryFunc, name) != null;
		}

		public bool CanResolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			throw new NotImplementedException();
		}

		public object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			if (dynamicContainer != null && dynamicContainer.CanResolve(type, name))
				return dynamicContainer.Resolve(type, name);

			var factory = RezolverCache.GetFactory(type, CreateFactoryFunc, name);
			if (factory == null)
				throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format("with name \"{0}\"", name) : ""));

			return factory(dynamicContainer);
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
			return _scope.Fetch(typeof (T), name);
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scopee, wwe don't support the call.
			if (create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}

		public T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			throw new NotImplementedException();
		}
	}
}