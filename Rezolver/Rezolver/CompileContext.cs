using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Represents the context in which a target is being compiled.
	/// </summary>
	public class CompileContext
	{
		private readonly IRezolver _rezolver;
		/// <summary>
		/// The rezolver that is considered the current compilation 'scope' for the purposes of looking up additional
		/// dependencies for the code being compiled.
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

		private MemberExpression _contextDynamicRezolverPropertyExpression;
		/// <summary>
		/// Returns an expression that representss reading the Rezolver property of the <see cref="RezolveContextParameter"/> to aid in 
		/// code generation.
		/// </summary>
		public MemberExpression ContextDynamicRezolverPropertyExpression
		{
			get
			{
				if(_contextDynamicRezolverPropertyExpression == null)
				{
					_contextDynamicRezolverPropertyExpression = Expression.Property(RezolveContextParameter, "DynamicRezolver");
				}
				return _contextDynamicRezolverPropertyExpression;
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

		private CompileContext(CompileContext parentContext)
		{
			parentContext.MustNotBeNull("parentContext");

			_compilingTargets = parentContext._compilingTargets;
			_rezolveContextParameter = parentContext._rezolveContextParameter;
			_rezolver = parentContext._rezolver;
		}

		/// <summary>
		/// Creates a new CompileContext
		/// </summary>
		/// <param name="rezolver">Required. Will be set into the <see cref="Rezolver"/> property.</param>
		/// <param name="targetType">Optional. Will be set into the <see cref="TargetType"/> property.</param>
		/// <param name="rezolveContextParameter">Optional.  Will be set into the <see cref="RezolveContextParameter"/>
		/// <param name="compilingTargets">Optional.  Allows you to seed the stack of compiling targets from creation.</param>
		/// property.</param>
		public CompileContext(IRezolver rezolver, Type targetType = null, ParameterExpression rezolveContextParameter = null, IEnumerable<IRezolveTarget> compilingTargets = null)
		{
			rezolver.MustNotBeNull("rezolver");

			_rezolver = rezolver;
			_targetType = targetType;
			_rezolveContextParameter = rezolveContextParameter;
			_compilingTargets = new Stack<IRezolveTarget>(compilingTargets ?? Enumerable.Empty<IRezolveTarget>());
		}

		/// <summary>
		/// Creates a new CompileContext using an existing one as a template.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, rezolver and rezolve context parameter properties.</param>
		/// <param name="targetType">The target type that is expected to be compiled.</param>
		public CompileContext(CompileContext parentContext, Type targetType = null)
			: this(parentContext)
		{
			_targetType = targetType;
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
