using System;
using System.Linq.Expressions;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Represents a target that is rezolved during expression building - i.e. bound early.  
	/// 
	/// That is, a target is located from the scope that is supplied to thee CreateExpression method,
	/// and that target is then used to donate the expression.
	/// 
	/// There should perhaps also be a late-bound version of this, which takes a container instead of a scope.
	/// 
	/// But since I'm not at the container level (yet), I can't do that.
	/// </summary>
	public class RezolvedTarget : RezolveTargetBase
	{
		//private readonly RezolverScopeExtensions.RezolveCallExpressionInfo _rezolveCall;
		private readonly Type _resolveType;
		private readonly Expression _resolveNameExpression;

		internal RezolvedTarget(RezolverScopeExtensions.RezolveCallExpressionInfo rezolveCall)
		{
			_resolveType = rezolveCall.Type;
			_resolveNameExpression = rezolveCall.Name;
		}

		public RezolvedTarget(Type type, Expression name = null)
		{
			type.MustNotBeNull("type");
			_resolveType = type;
			_resolveNameExpression = name;
		}

		public override Type DeclaredType
		{
			get { return _resolveType; }
		}

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			scope.MustNotBeNull("scope");
			//basic - without supporting a name
			var resolvedTarget = scope.Fetch(_resolveType, null);
			if(resolvedTarget == null)
				throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _resolveType));
			return Expression.Convert(resolvedTarget.CreateExpression(scope), targetType ?? _resolveType);
		}
	}
}