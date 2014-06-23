using System;
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

		public object Rezolve(Type type, string name = null, IRezolverScope dynamicScope = null)
		{
			var target = _scope.Fetch(type, name);
			if (target != null)
			{
				ParameterExpression scopeParamExp = Expression.Parameter(typeof (IRezolverScope), "scope");

				var dlg = Expression.Lambda<Func<IRezolverScope, object>>() target.CreateExpression(scopeParamExp)
			}
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			return _scope.Fetch(type, name);
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			if(create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}
	}
}