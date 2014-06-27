using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Versioning;
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
			
			var resolvedTarget = scopeContainer.Fetch(_resolveType, null);

			Func<object> compiledRezolveCall = null;
			Func<string> compiledNameCall = null;

			if (resolvedTarget != null)
			{
				var toCall = ExpressionHelper.GetFactoryForTarget(scopeContainer, targetType, resolvedTarget, currentTargets);
				compiledRezolveCall = () => toCall(null);
			}

			if (_resolveNameTarget != null)
			{
				//I think in this case, we *have* to defer to a dynamic resolve call on the scopeContainer in addition to 
				//intrinsic dynamic container because we can't know if the name target reprents a single value, or something which
				//produces lots of different values based on ambient environments.
				//There is the minority case for ObjectTarget and probably SingletonTarget,  which will always produce the 
				//same instance, but there's no reliable way - apart from a type test - to determine that.
				var toCall = ExpressionHelper.GetFactoryForTarget(scopeContainer, targetType, _resolveNameTarget, currentTargets);
				compiledNameCall = () => (string)toCall(null);
			}
			var helper = new LateBoundRezolveCall(targetType, compiledRezolveCall, compiledNameCall);
			var methodCall = MethodCallExtractor.ExtractCalledMethod(() => helper.Resolve(null));
				
			return Expression.Convert(Expression.Call(Expression.Constant(helper), methodCall, ExpressionHelper.DynamicContainerParam), targetType ?? _resolveType);
		}

		protected internal class LateBoundRezolveCall
		{
			private readonly Type _targetType;
			private readonly Func<object> _compiledResultFactory;
			private readonly Func<string> _nameFactory;

			public LateBoundRezolveCall(Type targetType, Func<object> compiledResultFactory, Func<string> name)
			{
				_targetType = targetType;
				_compiledResultFactory = compiledResultFactory;
				_nameFactory = name;
			}

			//note - when this call is made, the dynamic scope is the one that's passed
			public object Resolve(IRezolverContainer dynamicScope)
			{
				if (dynamicScope != null)
				{
					var name = _nameFactory != null ? _nameFactory() : null;
					if (dynamicScope.CanResolve(_targetType, name))
					{
						return dynamicScope.Rezolve(_targetType, _nameFactory());
					}
				}
				if(_compiledResultFactory == null)
					throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _targetType));

				return _compiledResultFactory;
			}
		}
	}
}