using Rezolver.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rezolver
{
    /// <summary>
    /// Default implementation of the <see cref="IRezolveTargetEntry"/> interface.
    /// 
    /// Supports multiple targets registered under one entry, with the last of those
    /// targets to be registered being treated as the default target.
    /// </summary>
	public class RezolveTargetEntry : IRezolveTargetEntry
	{
		private readonly Type _registeredType;
		private IRezolveTarget _defaultTarget;
		private ListTarget _listTarget;
		private List<IRezolveTarget> _targets;

		public virtual bool UseFallback { get { return false; } }

		public RezolveTargetEntry(Type registeredType, params IRezolveTarget[] targets)
		{
			registeredType.MustNotBeNull(nameof(registeredType));
			targets.MustNotBeNull(nameof(targets));
			if (targets.Any(t => t == null))
				throw new ArgumentException("All targets must be non-null", nameof(targets));

			_registeredType = registeredType;
			_defaultTarget = targets[targets.Length - 1];
			_targets = new List<IRezolveTarget>(targets);
		}

		public Type DeclaredType
		{
			get
			{
				return _registeredType;
			}
		}

		public IRezolveTarget DefaultTarget
		{
			get
			{
				return _defaultTarget;
			}
		}

		public IEnumerable<IRezolveTarget> Targets
		{
			get
			{
				return _targets.AsReadOnly();
			}
		}

		public void AddTarget(IRezolveTarget target, bool checkForDuplicates = false)
		{
            if (!checkForDuplicates || !_targets.Contains(target))
            {
                _targets.Add(target);
                _defaultTarget = target;
            }
		}

		public Expression CreateExpression(CompileContext context)
		{
			//here it will depend on the type in the compile context (once fully implemented, of course)
			//only way to figure that out is to find out which target supports the incoming type
			IRezolveTarget match = null;
			if(SupportsType(context.TargetType, out match))
				return match.CreateExpression(context);

			throw new ArgumentException(String.Format(Exceptions.TargetDoesntSupportType_Format, context.TargetType),
					nameof(context.TargetType));
		}

		public bool SupportsType(Type type)
		{
			//either the type is equal to the type that was used for the individual item.
			//or it could be an IEnumerable of that type.
			IRezolveTarget match = null;
			return SupportsType(type, out match);
		}

		private bool SupportsType(Type type, out IRezolveTarget match)
		{
			match = null;
			//either the type is equal to the type that was used for the individual item.
			//or it could be an IEnumerable of that type.

			//important thing here is that if we end up going to the IEnumerable, then we 
			//need to cache the target that we use.
			if (DefaultTarget.SupportsType(type))
			{
				match = DefaultTarget;
				return true;
			}

			if (TypeHelpers.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var elementType = TypeHelpers.GetGenericArguments(type)[0];
				//we only realise the list target if we haven't already created it
				if (_listTarget == null)
				{
					var temp = CreateListTarget();
					if (temp.SupportsType(type))
					{
						match = _listTarget = temp;
						return true;
					}
				}
				else
				{
					if (_listTarget.SupportsType(type))
					{
						match = _listTarget;
						return true;
					}
				}
			}

			return false;
		}

		private ListTarget CreateListTarget()
		{
			return new ListTarget(DeclaredType, _targets, true);
		}
	}
}