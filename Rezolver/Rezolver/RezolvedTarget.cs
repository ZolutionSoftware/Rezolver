using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null, ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			//TODO: Change how the expression is built based on whether a dynamiccontainerparameter expression is passed in
			scopeContainer.MustNotBeNull("scope");

			if (dynamicContainerExpression != null)
			{
				Func<object> compiledRezolveCall = null;
				Func<IRezolverContainer, string> compiledNameCall = null;

				if (_resolveNameTarget != null)
				{
					//I think in this case, we *have* to defer to a dynamic resolve call on the scopeContainer in addition to 
					//intrinsic dynamic container because we can't know if the name target reprents a single value, or something which
					//produces lots of different values based on ambient environments.
					//There is the minority case for ObjectTarget and probably SingletonTarget,  which will always produce the 
					//same instance, but there's no reliable way - apart from a type test - to determine that.
					//TODO: make this call generic after Resolve<T> has been added to the IRezolverContainer
					var toCall = ExpressionHelper.GetFactoryForTarget(scopeContainer, targetType, _resolveNameTarget, currentTargets);

					compiledNameCall = (dynamicScope) => (string)toCall(dynamicScope);
				}

				var resolvedTarget = scopeContainer.Fetch(DeclaredType, compiledNameCall != null ? compiledNameCall(null) : null);

				if (resolvedTarget != null)
				{
					var toCall = ExpressionHelper.GetFactoryForTarget(scopeContainer, targetType, resolvedTarget, currentTargets);
					//do not pass a dynamic container to the factory - because this particular expression is only intended
					//to work on this scope, not the dynaamic one.
					compiledRezolveCall = () => toCall(null);
				}
				//var helper = new LateBoundRezolveCall(targetType ?? DeclaredType, compiledRezolveCall, compiledNameCall);
				//only one way to do this - do all the checks now so that minimal decisions are made when the resolve operation
				//is called.
				Func<IRezolverContainer, object> lateBoundFounc;
				var finalType = targetType ?? DeclaredType;
				if (compiledNameCall != null)
				{
					if (compiledRezolveCall != null)
					{
						lateBoundFounc = (dynamicScope) =>
						{
							if (dynamicScope != null)
							{
								var name = compiledNameCall(dynamicScope);
								if (dynamicScope.CanResolve(finalType, name))
								{
									return dynamicScope.Resolve(finalType, name);
								}
							}
							return compiledRezolveCall();
						};
					}
					else
					{
						///same as above, but an exception is thrown if the dynamic scope can't resolve
						lateBoundFounc = (dynamicScope) =>
						{
							if (dynamicScope != null)
							{
								var name = compiledNameCall(dynamicScope);
								if (dynamicScope.CanResolve(finalType, name))
								{
									return dynamicScope.Resolve(finalType, name);
								}
							}
							throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, finalType));
						};
					}
				}
				else
				{
					if (compiledRezolveCall != null)
					{

						lateBoundFounc = (dynamicScope) =>
						{
							if (dynamicScope != null)
							{
								if (dynamicScope.CanResolve(finalType, null))
								{
									return dynamicScope.Resolve(finalType, null);
								}
							}

							return compiledRezolveCall();
						};
					}
					else
					{
						lateBoundFounc =
							(dynamicScope) =>
							{
								if (dynamicScope != null)
								{
									if (dynamicScope.CanResolve(finalType, null))
									{
										return dynamicScope.Resolve(finalType, null);
									}
								}

								throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, finalType));
							};
					}
				}

				return
					Expression.Call(Expression.Constant(lateBoundFounc), lateBoundFounc.GetType().GetMethod("Invoke"),
						dynamicContainerExpression);
			}
			else
			{

				string name = _resolveNameTarget != null
					? (string)scopeContainer.Compiler.CompileTarget(_resolveNameTarget, scopeContainer,null,currentTargets).GetObject()
					: null;
				var resolvedTarget = scopeContainer.Fetch(_resolveType, name);
				if (resolvedTarget == null)
					throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _resolveType));
				return resolvedTarget.CreateExpression(scopeContainer, targetType: targetType, currentTargets: currentTargets);
			}
		}

		protected internal class LateBoundRezolveCall
		{
			public static readonly MethodInfo ResolveMethodInfo =
				MethodCallExtractor.ExtractCalledMethod((LateBoundRezolveCall call) => call.Resolve(null));

			private readonly Type _targetType;
			private readonly Func<object> _compiledResultFactory;
			private readonly Func<IRezolverContainer, string> _nameFactory;

			public LateBoundRezolveCall(Type targetType, Func<object> compiledResultFactory, Func<IRezolverContainer, string> name)
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
					var name = _nameFactory != null ? _nameFactory(dynamicScope) : null;
					if (dynamicScope.CanResolve(_targetType, name))
					{
						return dynamicScope.Resolve(_targetType, name);
					}
				}
				if (_compiledResultFactory == null)
					throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _targetType));

				return _compiledResultFactory();
			}


		}
	}
}