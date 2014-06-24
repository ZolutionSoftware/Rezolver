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

		public object Rezolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			var target = _scope.Fetch(type, name);
			if (target != null)
			{
				var factoryExp = Expression.Lambda<Func<IRezolverContainer, object>>(target.CreateExpression(_scope, type),
					ExpressionHelper.DynamicContainerParam);
#if DEBUG
				Debug.WriteLine("RezolverContainer is Compiling lambda \"{0}\" for type {1}", factoryExp, type);
#endif
				var dlg = factoryExp.Compile();
				return dlg(dynamicContainer);
			}
			throw new ArgumentException(string.Format("No target could be found for the type {0}{1}", type, name != null ? string.Format(", name \"{0}\"", name) : ""));
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