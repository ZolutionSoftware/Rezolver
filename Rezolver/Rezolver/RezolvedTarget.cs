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
		private readonly Expression _resolveNameExpression;

		public Expression Name { get { return _resolveNameExpression; } }

		internal RezolvedTarget(RezolveTargetAdapter.RezolveCallExpressionInfo rezolveCall)
		{
			_resolveType = rezolveCall.Type;
			_resolveNameExpression = rezolveCall.Name;
		}

		public RezolvedTarget(Type type, string name = null)
			: this(type, name != null ? Expression.Constant(name) : null)
		{

		}

		public RezolvedTarget(Type type, Expression name)
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