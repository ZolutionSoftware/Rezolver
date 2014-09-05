using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Represents the context in which a target is being compiled.
	/// 
	/// TODO: Wondering if RezolveContext should be passed into the CompileContext...  Everything tells me it should.
	/// </summary>
	public class CompileContext
	{
		/// <summary>
		/// Key for a shared expression used during expression tree generation
		/// </summary>
		public class SharedExpressionKey : IEquatable<SharedExpressionKey>
		{
			public Type RequestingType { get; private set; }
			public Type TargetType { get; private set; }
			public string Name { get; private set; }

			public SharedExpressionKey(Type targetType, string name, Type requestingType = null)
			{
				targetType.MustNotBeNull("targetType");
				name.MustNotBeNull("name");
				TargetType = targetType;
				Name = name;
				RequestingType = requestingType;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;
				return base.Equals(obj as SharedExpressionKey);
			}

			public override int GetHashCode()
			{
				return TargetType.GetHashCode() ^
					Name.GetHashCode() ^
					(RequestingType != null ? RequestingType.GetHashCode() : 0);
			}

			public bool Equals(SharedExpressionKey other)
			{
				return object.ReferenceEquals(this, other) ||
					(RequestingType == other.RequestingType && TargetType == other.TargetType && Name == other.Name);
			}
		}

		private readonly IRezolver _rezolver;
		/// <summary>
		/// The rezolver that is considered the current compilation 'scope' for the purposes of looking up additional
		/// dependencies for the code being compiled.  Note that this is not necessarily the same as the rezolver that
		/// might be passed in the RezolveContext to the generated code at runtime, as compiled targets might be retargeted
		/// to other rezolvers for dependency resolution.
		/// </summary>
		public IRezolver Rezolver { get { return _rezolver; } }

		private readonly Type _targetType;
		/// <summary>
		/// The desired type to be returned by the generated code.
		/// </summary>
		public Type TargetType { get { return _targetType; } }

		private readonly ParameterExpression _rezolveContextParameter;
		/// <summary>
		/// A expression to be used to bind to the RezolveContext that will be passed to the generated code at runtime.
		/// 
		/// If this is never set, then the framework will use <see cref="ExpressionHelper.RezolveContextParameter"/> by default.
		/// 
		/// In theory, you should never need to set this to anything else, unless you're doing something very interesting with
		/// the generated expressions.
		/// </summary>
		public ParameterExpression RezolveContextParameter { get { return _rezolveContextParameter ?? ExpressionHelper.RezolveContextParameter; } }

		private readonly Stack<IRezolveTarget> _compilingTargets;


		private MemberExpression _contextRezolverPropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the Rezolver property of the <see cref="RezolveContextParameter"/> to aid in 
		/// code generation.
		/// 
		/// Note that this is always non-null.
		/// </summary>
		public MemberExpression ContextRezolverPropertyExpression
		{
			get
			{
				if(_contextRezolverPropertyExpression == null)
				{
					_contextRezolverPropertyExpression = Expression.Property(RezolveContextParameter, "Rezolver");
				}
				return _contextRezolverPropertyExpression;
			}
		}

		private MemberExpression _contextScopePropertyExpression;
		/// <summary>
		/// Returns an expression that represents reading the Scope property of the <see cref="RezolveContext"/>
		/// </summary>
		public MemberExpression ContextScopePropertyExpression
		{
			get
			{
				if (_contextScopePropertyExpression == null)
					_contextScopePropertyExpression = Expression.Property(RezolveContextParameter, "Scope");
				return _contextScopePropertyExpression;
			}
		}

		//TODO: add property expression getters for the other properties on RezolveContext.

		/// <summary>
		/// An enumerable representing the current stack of targets that are being compiled.
		/// 
		/// The underlying stack is not exposed through this enumerable.
		/// </summary>
		public IEnumerable<IRezolveTarget> CompilingTargets { get { return _compilingTargets.ToArray(); } }

		private Dictionary<SharedExpressionKey, Expression> _sharedExpressions;

		/// <summary>
		/// Shared expressions are expressions that targets add to the compile context as they are compiled,
		/// enabling them to generate code which is both more efficient at runtime (e.g. avoiding the creation of
		/// redundant locals for blocks which can reuse a pre-existing local) and that can be more efficiently rewritten 
		/// - due to the reuse of identical expression references for things like conditional checks and so on.
		/// 
		/// A compiler MUST handle the case where this enumerable contains ParameterExpressions, as they will need
		/// to be added as local variables to an all-encompassing BlockExpression around the root of an expression tree 
		/// that is to be compiled.
		/// </summary>
		public IEnumerable<Expression> SharedExpressions
		{
			get
			{
				return _sharedExpressions.Values;
			}
		}



		private CompileContext(CompileContext parentContext, bool inheritSharedExpressions)
		{
			parentContext.MustNotBeNull("parentContext");

			_compilingTargets = parentContext._compilingTargets;
			_rezolveContextParameter = parentContext._rezolveContextParameter;
			_rezolver = parentContext._rezolver;
			_sharedExpressions = inheritSharedExpressions ? parentContext._sharedExpressions : new Dictionary<SharedExpressionKey, Expression>();
		}

		/// <summary>
		/// Creates a new CompileContext
		/// </summary>
		/// <param name="rezolver">Required. Will be set into the <see cref="Rezolver"/> property.</param>
		/// <param name="targetType">Optional. Will be set into the <see cref="TargetType"/> property.</param>
		/// <param name="rezolveContextParameter">Optional.  Will be set into the <see cref="RezolveContextParameter"/>
		/// <param name="compilingTargets">Optional.  Allows you to seed the stack of compiling targets from creation.</param>
		/// <param name="enableDynamicRezolver">Optional.  If true, then rezolve targets that generate code which needs to resolve 
		/// additional dependencies from a rezolver should take into account any dynamic rezolver that might be passed in 
		/// the RezolveContext to the built code at runtime.  The target can use the <see cref="ContextRezolverPropertyExpression"/>
		/// for this purpose.  True is the default.  If you do not intend to use dynamic rezolvers you can switch this off,
		/// and you'll get a small performance improvement in some situations (around 20% improvement).</param>
		/// property.</param>
		public CompileContext(IRezolver rezolver, 
			Type targetType = null, 
			ParameterExpression rezolveContextParameter = null, 
			IEnumerable<IRezolveTarget> compilingTargets = null)
		{
			rezolver.MustNotBeNull("rezolver");

			_rezolver = rezolver;
			_targetType = targetType;
			_rezolveContextParameter = rezolveContextParameter;
			_compilingTargets = new Stack<IRezolveTarget>(compilingTargets ?? Enumerable.Empty<IRezolveTarget>());
			_sharedExpressions = new Dictionary<SharedExpressionKey, Expression>();
		}

		/// <summary>
		/// Creates a new CompileContext using an existing one as a template.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, rezolver and rezolve context parameter properties.</param>
		/// <param name="targetType">The target type that is expected to be compiled.</param>
		/// <param name="inheritSharedExpressions">If true, then the <see cref="SharedExpressions"/> for this context will be shared
		/// from the parent context - meaning that any new additions will be added back to the parent context again.  The default is
		/// false, however if you are chaining multiple targets' expressions together you will need to pass true.</param>
		public CompileContext(CompileContext parentContext, Type targetType = null, bool inheritSharedExpressions = false)
			: this(parentContext, inheritSharedExpressions)
		{
			_targetType = targetType;
		}

		private Dictionary<Type, Dictionary<string, ParameterExpression>> CloneSharedLocals(Dictionary<Type, Dictionary<string, ParameterExpression>> source)
		{
			Dictionary<Type, Dictionary<string, ParameterExpression>> toReturn = new Dictionary<Type,Dictionary<string,ParameterExpression>>();
			Dictionary<string, ParameterExpression> newDictionary;
			foreach(var kvp in source)
			{
				newDictionary = new Dictionary<string, ParameterExpression>();
				foreach(var kvp2 in kvp.Value)
				{
					newDictionary[kvp2.Key] = kvp2.Value;
				}
				toReturn[kvp.Key] = newDictionary;
			}

			return toReturn;
		}

		/// <summary>
		/// Retrieves
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="requestingType"></param>
		/// <returns></returns>
		public ParameterExpression GetOrAddSharedLocal(Type type, string name, Type requestingType = null)
		{
			try { 
			return (ParameterExpression)GetOrAddSharedExpression(type, name, () => Expression.Parameter(type, name), requestingType);
				}
			catch(InvalidCastException)
			{
				throw new InvalidOperationException("Cannot add ParameterExpression: A shared expression of a different expression type has already been added with the same parameters.");
			}
		}

		public Expression GetOrAddSharedExpression(Type type, string name, Func<Expression> expressionFactory, Type requestingType = null)
		{
			type.MustNotBeNull("type");
			expressionFactory.MustNotBeNull("expressionFactory");
			Expression toReturn;
			//if this is 
			SharedExpressionKey key = new SharedExpressionKey(type, name, requestingType);
			if (!_sharedExpressions.TryGetValue(key, out toReturn))
				_sharedExpressions[key] = toReturn = expressionFactory();
			return toReturn;
		}

		/// <summary>
		/// Adds the target to the compilation stack if it doesn't already exist.
		/// 
		/// The method returns whether the target was added.
		/// </summary>
		/// <param name="toCompile"></param>
		/// <returns></returns>
		public bool PushCompileStack(IRezolveTarget toCompile)
		{
			toCompile.MustNotBeNull("toCompile");

			if(!_compilingTargets.Contains(toCompile))
			{
				_compilingTargets.Push(toCompile);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Call this to find out if a target is currently compiling without trying
		/// to also add it to the stack.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsCompiling(IRezolveTarget target)
		{
			return _compilingTargets.Contains(target);
		}

		/// <summary>
		/// Pops a target from the stack and returns it.  Note that if there
		/// are no targets on the stack, an InvalidOperationException will occur.
		/// </summary>
		/// <returns></returns>
		public IRezolveTarget PopCompileStack()
		{
			return _compilingTargets.Pop();
		}
	}
}
