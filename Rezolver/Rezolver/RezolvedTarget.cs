using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Represents a target that is rezolved during expression building.
	/// 
	/// That is, a target is located from the scope that is supplied to the CreateExpression method,
	/// and that target is then used to donate the expression.
	/// 
	/// There should perhaps also be a late-bound version of this, which takes a container instead of a scope.
	/// 
	/// But since I'm not at the container level (yet), I can't do that.
	/// </summary>
	public class RezolvedTarget : RezolveTargetBase
	{
		private readonly Type _resolveType;
		private readonly IRezolveTarget _resolveNameTarget;

		public IRezolveTarget Name { get { return _resolveNameTarget; } }

		internal RezolvedTarget(RezolveTargetAdapter.RezolveCallExpressionInfo rezolveCall)
		{
			_resolveType = rezolveCall.Type;
			_resolveNameTarget = rezolveCall.Name;
		}

		public RezolvedTarget(Type type, string name = null)
			: this(type, name != null ? name.AsObjectTarget() : null)
		{

		}

		public RezolvedTarget(Type type, IRezolveTarget name)
		{
			type.MustNotBeNull("type");
			_resolveType = type;
			_resolveNameTarget = name;
		}

		public override Type DeclaredType
		{
			get { return _resolveType; }
		}

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null, Stack<IRezolveTarget> currentTargets = null)
		{
			scopeContainer.MustNotBeNull("scope");

			if (_resolveNameTarget != null)
			{
				//I think in this case, we *have* to defer to a dynamic resolve call on the scopeContainer in addition to 
				//intrinsic dynamic container because we can't know if the name target reprents a single value, or something which
				//produces lots of different values based on ambient environments.
				//There is the minority case for ObjectTarget and probably SingletonTarget,  which will always produce the 
				//same instance.
			}
			
			var resolvedTarget = scopeContainer.Fetch(_resolveType, null);

			//TODO: Build this target's lambda and compile it to be passed as a constructor parameter to the LateBounddRezolver

			//var rezolvedHelper = new RezolvedHelper(targetType ?? DeclaredType, resolvedTarget, _resolveNameTarget);
			
			if(resolvedTarget == null)
				throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _resolveType));
			return Expression.Convert(resolvedTarget.CreateExpression(scopeContainer, currentTargets: currentTargets), targetType ?? _resolveType);
		}

		protected internal class LateBoundRezolveCall<TTarget>
		{
			private readonly Func<TTarget> _compiledResultFactory;
			private readonly Func<string> _nameFactory;

			public LateBoundRezolveCall(Func<TTarget> compiledResultFactory, Func<string> name)
			{
				_compiledResultFactory = compiledResultFactory;
				_nameFactory = name;
			}

			//note - when this call is made, the dynamic scope is the one that's passed
			private TTarget Resolve(IRezolverContainer dynamicScope)
			{
				var nameResult = _nameFactory();
				if (dynamicScope != null && dynamicScope.CanResolve(typeof (TTarget), _nameFactory))
				{
					return (TTarget)dynamicScope.Rezolve(typeof(TTarget), _name);
				}
			}
		}
	}


}