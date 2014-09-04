﻿using System;
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

		private Dictionary<Type, Dictionary<string, ParameterExpression>> _sharedLocals;

		/// <summary>
		/// Shared locals are expressions that targets add to the compile context as they are compiled,
		/// enabling them to generate code that references local variables.  The concept of them being shared
		/// is that once a target's compiled code is finished executing, another target's code is free to use
		/// it for itself.  This is typically done amongst different instances of the same type of 
		/// IRezolveTarget implementation.
		/// </summary>
		public IEnumerable<ParameterExpression> SharedLocals
		{
			get
			{
				foreach(var kvp in _sharedLocals)
				{
					foreach(var kvp2 in kvp.Value)
					{
						yield return kvp2.Value;
					}
				}
			}
		}


		private CompileContext(CompileContext parentContext, bool inheritLocals)
		{
			parentContext.MustNotBeNull("parentContext");

			_compilingTargets = parentContext._compilingTargets;
			_rezolveContextParameter = parentContext._rezolveContextParameter;
			_rezolver = parentContext._rezolver;
			_sharedLocals = inheritLocals ? parentContext._sharedLocals : new Dictionary<Type, Dictionary<string, ParameterExpression>>();
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
			_sharedLocals = new Dictionary<Type, Dictionary<string, ParameterExpression>>();
		}

		/// <summary>
		/// Creates a new CompileContext using an existing one as a template.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, rezolver and rezolve context parameter properties.</param>
		/// <param name="targetType">The target type that is expected to be compiled.</param>
		/// <param name="inheritLocals">If true, then the <see cref="SharedLocals"/> for this context will be shared
		/// from the parent context - meaning that any new additions will be added back to the parent context again.  The default is
		/// false, however if you are chaining multiple targets' expressions together you will need to pass true.</param>
		public CompileContext(CompileContext parentContext, Type targetType = null, bool inheritLocals = false)
			: this(parentContext, inheritLocals)
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

		public ParameterExpression GetOrAddSharedLocal(Type type, string name)
		{
			ParameterExpression toReturn;
			Dictionary<string, ParameterExpression> targetDictionary;
			if (!_sharedLocals.TryGetValue(type, out targetDictionary))
				_sharedLocals[type] = targetDictionary = new Dictionary<string, ParameterExpression>();
			if (!targetDictionary.TryGetValue(name, out toReturn))
				targetDictionary[name] = toReturn = Expression.Parameter(type, name);
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
