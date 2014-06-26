using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// </summary>
	public class RezolverContainer : IRezolverContainer
	{
		private readonly IRezolverScope _scope;

		public RezolverContainer(IRezolverScope scope)
		{
			//TODO: check for null scope.
			_scope = scope;
		}

		public bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//REVIEW: 
			if (dynamicContainer != null && dynamicContainer.CanResolve(type, name, null))
				return true;
			return Fetch(type, name) != null;
		}

		public object Rezolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			var target = _scope.Fetch(type, name);
			if (target != null)
			{
				//slight issue here - if the target expression returns value type, then we're forcing a box/unbox
				//on this delegate and when it's called for the caller.
				//this perhaps could be fixed with a generic version of Rezolve

				//notice that despite the dynamic container being passed to this method, it's not the scope
				//that's provided to the CreateExpression call.  Instead, passing that container is deferred
				//until we execute the the delegate itself.  To support such dynamic scoping, a target must simply
				//reference the parameter expression ExpressionHelper.DynamicContainerParam.
				var factoryExp = ExpressionHelper.GetLambdaForTarget(this, type, target);
#if DEBUG
				Debug.WriteLine("RezolverContainer is Compiling lambda \"{0}\" for type {1}", factoryExp, type);
#endif
				var dlg = factoryExp.Compile();

				//TODO: implement caching.  Probably want to abstract that away so that on more 'capable' platforms we can use ConcurrentDictionary
				return dlg(dynamicContainer);
			}
			throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format(", name \"{0}\"", name) : ""));
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

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scopee, wwe don't support the call.
			if(create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}
	}
}