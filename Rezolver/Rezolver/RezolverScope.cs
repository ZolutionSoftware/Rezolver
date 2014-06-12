using System;
using System.Collections.Generic;

namespace Rezolver
{
	public class RezolverScope : IRezolverScope
	{
		//TODO: extract an abstract base implementation of this class that does away with the dictionary and lockers, with extension points in place of those to allow for future expansion.
		#region immediate type entries

		private readonly object _targetsLocker = new object();
		private readonly Dictionary<Type, IRezolveTarget> _targets = new Dictionary<Type,IRezolveTarget>();

		#endregion

		#region named child scopes

		private readonly object _namedScopesLocker = new object();
		private readonly Dictionary<string, INamedRezolverScope> _namedScopes = new Dictionary<string, INamedRezolverScope>();
 
		#endregion

		public void Register(IRezolveTarget target, Type type = null, string name = null)
		{
			//TODO: Support name hierarchies by splitting the name by forward-slash and recursively creating a named-scope tree.

			if (name != null)
			{
				//get the named scope.  If it doesn't exist, create one.
				var namedScope = GetOrCreateNamedScope(target, type, name);
				//note here we don't pass the name through.
				//when we support named scopes, we would be lopping off the first item in a hierarchical name to allow for the recursion.
				namedScope.Register(target, type);
				return;
			}

			if (type == null)
				type = target.DeclaredType;



			if (target.SupportsType(type))
			{
				lock(_targetsLocker)
				{
					if (_targets.ContainsKey(type))
						throw new ArgumentException(string.Format(Resources.Exceptions.TypeIsAlreadyRegistered, type), "type");
					_targets[type] = target;
				}
			}
			else
				throw new ArgumentException(string.Format(Resources.Exceptions.TargetDoesntSupportType_Format, type), "target");
		}

		protected virtual INamedRezolverScope GetOrCreateNamedScope(IRezolveTarget target, Type type, string name)
		{
			//TODO: test that whitespace is stripped from front and back of whole string and individual names
			var nameHierarchy = name.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if(nameHierarchy.Length == 0)
				throw new ArgumentException("Name must not be empty");
			INamedRezolverScope namedScope;
			lock (_namedScopesLocker)
			{
				if (!_namedScopes.TryGetValue(name, out namedScope))
					_namedScopes[name] = namedScope = CreateNamedScope(name, target, type);
			}
			return namedScope;
		}

		/// <summary>
		/// Retrieves a scope matching the given name.  Returns null if no scope exists.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected internal INamedRezolverScope GetNamedScopeInternal(string name)
		{
			INamedRezolverScope toReturn;
			lock (_namedScopesLocker)
			{
				_namedScopes.TryGetValue(name, out toReturn);
			}
			return toReturn;
		}

		/// <summary>
		/// Called to create a new instance of a Named Scope with the given name, optionally for the given target and type.
		/// </summary>
		/// <param name="name">The name of the scope to create.</param>
		/// <param name="target">Optional - a target that is to be added to the named scope after creation.</param>
		/// <param name="type">Optional - the type for which a target will added after creation.</param>
		/// <returns></returns>
		protected virtual INamedRezolverScope CreateNamedScope(string name, IRezolveTarget target, Type type)
		{
			//base class simply creates a NamedRezolverScope
			return new NamedRezolverScope(this, name);
		}

		public IRezolveTarget Fetch(Type type, string name)
		{
			type.MustNotBeNull("type");

			if (name != null)
			{
				INamedRezolverScope namedScope;
				lock (_namedScopesLocker)
				{
					_namedScopes.TryGetValue(name, out namedScope);
				}
// ReSharper disable once PossibleNullReferenceException
				return _namedScopes == null ? null : namedScope.Fetch(type);
			}

			lock (_targetsLocker)
			{
				IRezolveTarget target;
				return _targets.TryGetValue(type, out target) ? target : null;
			}
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			if (create)
			{
				return GetOrCreateNamedScope(null, null, path);
			}
			else
			{
				return GetNamedScopeInternal(path);
			}
		}
	}
}
