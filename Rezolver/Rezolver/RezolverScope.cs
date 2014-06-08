using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class RezolverScope : IRezolverScope
	{
		private readonly object _locker = new object();
		private Dictionary<Type, IRezolveTarget> _targets = new Dictionary<Type,IRezolveTarget>();

		public void Register(IRezolveTarget target, Type type = null, string name = null)
		{
			if (type == null)
				type = target.DeclaredType;

			if (target.SupportsType(type))
			{
				lock(_locker)
				{
					if (_targets.ContainsKey(type))
						throw new ArgumentException(string.Format(Resources.Exceptions.TypeIsAlreadyRegistered, type), "type");
					_targets[type] = target;
				}
			}
			else
				throw new ArgumentException(string.Format(Resources.Exceptions.TargetDoesntSupportType_Format, type), "target");
		}

		public IRezolveTarget Fetch(Type type, string name)
		{
			type.MustNotBeNull("type");
			lock (_locker)
			{
				IRezolveTarget target;
				if (_targets.TryGetValue(type, out target))
					return target;
				else
					return null;
			}
		}
	}
}
