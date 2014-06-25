using System;
using System.Linq.Expressions;
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

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null)
		{
			scopeContainer.MustNotBeNull("scope");

			//the decision here is whether we resolve through the container or grab an expression from the scope underlying
			if (_resolveNameTarget != null)
			{
				
			}
			//basic - without supporting a name
			var resolvedTarget = scopeContainer.Fetch(_resolveType, null);

			var rezolvedHelper = new RezolvedHelper(targetType ?? DeclaredType, resolvedTarget, _resolveNameTarget);

			//if(resolvedTarget == null)
			//	throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _resolveType));
			return Expression.Convert(resolvedTarget.CreateExpression(scopeContainer), targetType ?? _resolveType);
		}

		protected internal class RezolvedHelper
		{
			private readonly Type _type;
			private readonly IRezolveTarget _resolvedTarget;
			private readonly IRezolveTarget _resolveNameTarget;

			public RezolvedHelper(Type type, IRezolveTarget resolvedTarget, IRezolveTarget resolveNameTarget)
			{
				_type = type;
				_resolvedTarget = resolvedTarget;
				_resolveNameTarget = resolveNameTarget;
			}

			private T GetObject<T>(IRezolverContainer containerScope, bool hasDefault, T defaultObject)
			{
				if(containerScope != null && containerScope.)
			}
		}
	}


}