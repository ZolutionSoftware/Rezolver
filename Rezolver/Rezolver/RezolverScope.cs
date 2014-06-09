using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Tests;

namespace Rezolver
{
	public class RezolverScope : IRezolverScope
	{
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
				INamedRezolverScope namedScope;
				lock (_namedScopesLocker)
				{
					if(!_namedScopes.TryGetValue(name, out namedScope))
						_namedScopes[name] = namedScope = CreateNamedScope(name, target, type);
				}
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
	}
}
