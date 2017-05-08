// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation
{
	/// <summary>
	/// Core implementation of <see cref="ICompileContext" />.  A root context (i.e. where <see cref="ParentContext"/> is
	/// <c>null</c>; created via the <see cref="CompileContext.CompileContext(IResolveContext, ITargetContainer, Type)"/>
	/// constructor) is the starting point for all shared state, such as the <see cref="Container"/> and the compilation
	/// stack.
	/// 
	/// The <see cref="ITargetContainer" /> implementation is done by decorating a new <see cref="OverridingTargetContainer" />
    /// which wraps the actual target container which contains the targets which will be compiled,
	/// so that new registrations can be added without interfering with upstream containers.
	/// 
	/// Note that many of the interface members are implemented explicitly - therefore most of your interaction with 
	/// this type is through its implementation of <see cref="ICompileContext"/> and <see cref="ITargetContainer"/>.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.ICompileContext" />
	/// <seealso cref="Rezolver.ITargetContainer" />
	/// <remarks>Note that you can only create an instance of this either through inheritance, via the explicit implementation
	/// of <see cref="ICompileContext.NewContext(Type, ScopeBehaviour?, ScopePreference?)"/>, or (preferably) via the
    /// <see cref="ITargetCompiler.CreateContext(IResolveContext, ITargetContainer)"/> method of an 
    /// <see cref="ITargetCompiler" /> resolved from an <see cref="IContainer" />.
	/// </remarks>
	public class CompileContext : ICompileContext, ITargetContainer
	{
		/// <summary>
		/// Gets the parent context from which this context was created, if applicable.
		/// </summary>
		/// <value>The parent context.</value>
		public ICompileContext ParentContext { get; }

        private readonly IResolveContext _resolveContext;
        /// <summary>
        /// Implementation of <see cref="ICompileContext.ResolveContext"/>
        /// </summary>
        public IResolveContext ResolveContext => _resolveContext ?? ParentContext?.ResolveContext;

		private readonly Type _targetType;
		/// <summary>
		/// Any <see cref="ICompiledTarget" /> built for a <see cref="ITarget" /> with this context should target this type.
		/// If null, then the <see cref="ITarget.DeclaredType" /> of the target being compiled should be used.
		/// </summary>
		/// <remarks>Note that when creating a child context with a null <c>targetType</c> argument, this property will be inherited
		/// from the <see cref="ParentContext"/>.</remarks>
		public Type TargetType { get { return _targetType ?? ParentContext?.TargetType; } }

		/// <summary>
		/// Implementation of <see cref="ICompileContext.ScopeBehaviourOverride"/>
		/// </summary>
		public ScopeBehaviour? ScopeBehaviourOverride { get; }

        private ScopePreference? _scopePreferenceOverride;
        /// <summary>
        /// Implementation of <see cref="ICompileContext.ScopePreferenceOverride"/>
        /// </summary>
        public ScopePreference? ScopePreferenceOverride { get { return _scopePreferenceOverride ?? ParentContext?.ScopePreferenceOverride; } }
		/// <summary>
		/// This is the <see cref="ITargetContainer"/> through which dependencies are resolved by this context in its 
		/// implementation of <see cref="ITargetContainer"/>.
		/// 
		/// In essence, this class acts as a decorator for this inner target container.
		/// </summary>
		protected ITargetContainer DependencyTargetContainer { get; }

		private readonly Stack<CompileStackEntry> _compileStack;

		/// <summary>
		/// Gets the stack entries for all the targets that are being compiled for all contexts
		/// related to this one - both up and down the hierarchy.
		/// </summary>
		/// <value>The compile stack.</value>
		public IEnumerable<CompileStackEntry> CompileStack => _compileStack?.AsReadOnly() ?? ParentContext?.CompileStack;

		/// <summary>
		/// Creates a new <see cref="CompileContext"/> as a child of another.
		/// </summary>
		/// <param name="parentContext">Used to seed the compilation stack, container, dependency container (which
		/// will still be wrapped in a new <see cref="OverridingTargetContainer"/> for isolation) and, optionally, 
		/// the target type (unless you pass a non-null type for <paramref name="targetType" />, which would
		/// override that).</param>
		/// <param name="targetType">The target type that is expected to be compiled, or null if the <see cref="TargetType"/>
		/// is to be inherited from the <paramref name="parentContext"/>.</param>
		/// <param name="scopeBehaviourOverride">Override the scope behaviour to be used for the target that is compiled with this context.</param>
        /// <param name="scopePreferenceOverride">Allows you to override the scope preference for any target being compiled in this context - 
        /// if not provided, then it is automatically inherited from the parent.</param>
		protected CompileContext(ICompileContext parentContext, 
            Type targetType = null, 
            ScopeBehaviour? scopeBehaviourOverride = null,
            ScopePreference? scopePreferenceOverride = null)
		{
			parentContext.MustNotBeNull(nameof(parentContext));
			ParentContext = parentContext;
			DependencyTargetContainer = new OverridingTargetContainer(parentContext);
			_targetType = targetType;
            _resolveContext = parentContext.ResolveContext.New(newRequestedType: targetType);
			ScopeBehaviourOverride = scopeBehaviourOverride;
            _scopePreferenceOverride = scopePreferenceOverride;
			//note - many of the other members are inherited in the property getters or interface implementations
		}

		/// <summary>
		/// Creates a new CompileContext
		/// </summary>
		/// <param name="resolveContext">Required.  The context for which compilation is being performed.</param>
		/// <param name="dependencyTargetContainer">Required - An <see cref="ITargetContainer" /> that contains the <see cref="ITarget" />s that
		/// will be required to complete compilation.
        /// 
		/// Note - this argument is passed to a new <see cref="OverridingTargetContainer" /> that is created and proxied by this class' implementation
		/// of <see cref="ITargetContainer" />.
        /// 
		/// As a result, it's possible to register new targets directly into the context via its implementation of <see cref="ITargetContainer"/>,
		/// without modifying the underlying targets in the container you pass.</param>
		/// <param name="targetType">Optional. Will be set into the <see cref="TargetType" /> property.  If null, then any 
		/// <see cref="ITarget"/> that is compiled should be compiled for its own <see cref="ITarget.DeclaredType"/>.</param>
		protected CompileContext(IResolveContext resolveContext,
		  ITargetContainer dependencyTargetContainer,
		  Type targetType = null
		  )
		{
			resolveContext.MustNotBeNull(nameof(resolveContext));
			dependencyTargetContainer.MustNotBeNull(nameof(dependencyTargetContainer));
			DependencyTargetContainer = new OverridingTargetContainer(dependencyTargetContainer);
            _resolveContext = resolveContext;
			_targetType = targetType;
			_compileStack = new Stack<CompileStackEntry>(30);
		}

        ICompileContext ICompileContext.NewContext(Type targetType, 
            ScopeBehaviour? scopeBehaviourOverride,
            ScopePreference? scopePreferenceOverride)
		{
			return NewContext(targetType, scopeBehaviourOverride);
		}

        /// <summary>
        /// Used by the explicit implementation of <see cref="ICompileContext.NewContext(Type, ScopeBehaviour?, ScopePreference?)"/>.
        /// 
        /// Override this in your derived class to create the correct implementation of <see cref="ICompileContext"/>.
        /// </summary>
        /// <param name="targetType">Optional.  The type for which the target is to be compiled, if different from this 
        /// context's <see cref="TargetType" />.</param>
        /// <param name="scopeBehaviourOverride">Override the scope behaviour to be used for the target that is compiled with the new context.</param>
        /// <param name="scopePreferenceOverride">Sets the <see cref="ScopePreferenceOverride"/>.  As soon as this is set on one context, it is automatically 
        /// inherited by all its child contexts (i.e. you cannot null it)</param>
        protected virtual ICompileContext NewContext(Type targetType = null, 
            ScopeBehaviour? scopeBehaviourOverride = null,
            ScopePreference? scopePreferenceOverride = null)
		{
			return new CompileContext(this, targetType, scopeBehaviourOverride, scopePreferenceOverride ?? ScopePreferenceOverride);
		}

		/// <summary>
		/// Adds the target to the compilation stack if it doesn't already exist.
		/// </summary>
		/// <param name="toCompile">The target to be pushed</param>
		/// <param name="targetType">The type for which the target is being compiled, if different from <see cref="ITarget.DeclaredType" /></param>
		/// <remarks>Targets can appear on the compilation stack more than once for different types, since the <see cref="ICompiledTarget" />
		/// produced for a target for one type can be different than it is for another.  Ultimately, if a target does in fact have a
		/// cyclic dependency graph, then this method will detect that.</remarks>
		bool ICompileContext.PushCompileStack(ITarget toCompile, Type targetType)
		{
			//when referring this call up to the parent, we either use the targetType passed, or default to our target type
			if (ParentContext != null)
				return ParentContext.PushCompileStack(toCompile, targetType ?? TargetType);

			toCompile.MustNotBeNull("toCompile");
			//whereas here we default to the target's declared type if no targetType is passed.
			CompileStackEntry entry = new CompileStackEntry(toCompile, targetType ?? toCompile.DeclaredType);
			if (!_compileStack.Contains(entry))
			{
				_compileStack.Push(entry);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Pops a target from the stack and returns it.  Note that if there
		/// are no targets on the stack, an <see cref="InvalidOperationException"/> will occur.
		/// </summary>
		/// <returns>The <see cref="CompileStackEntry"/> that was popped off the compilation stack.</returns>
		/// <remarks>If <see cref="ParentContext"/> is not null, then the call is redirected to that context, so that 
		/// the compilation stack is always shared between all contexts spawned from the same root.</remarks>
		CompileStackEntry ICompileContext.PopCompileStack()
		{
			if (ParentContext != null)
				return ParentContext.PopCompileStack();

			return _compileStack.Pop();
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.Register(ITarget, Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="target">See <see cref="ITargetContainer.Register(ITarget, Type)"/> for more</param>
		/// <param name="serviceType">See <see cref="ITargetContainer.Register(ITarget, Type)"/> for more</param>
		void ITargetContainer.Register(ITarget target, Type serviceType)
		{
			DependencyTargetContainer.Register(target, serviceType);
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.Fetch(Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="type">See <see cref="ITargetContainer.Fetch(Type)"/> for more.</param>
		ITarget ITargetContainer.Fetch(Type type)
		{
			return DependencyTargetContainer.Fetch(type);
		}

		/// <summary>
		/// Implements <see cref="ITargetContainer.FetchAll(Type)"/> by wrapping around the child target container created by this context on construction.
		/// </summary>
		/// <param name="type">See <see cref="ITargetContainer.FetchAll(Type)"/> for more</param>
		IEnumerable<ITarget> ITargetContainer.FetchAll(Type type)
		{
			return DependencyTargetContainer.FetchAll(type);
		}

		/// <summary>
		/// Always throws a <see cref="NotSupportedException"/>
		/// </summary>
		/// <param name="existing">Ignored</param>
		/// <param name="type">Ignored</param>
		/// <exception cref="NotSupportedException">Always thrown</exception>
		ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}

        ITargetContainer ITargetContainer.FetchContainer(Type type)
        {
            return DependencyTargetContainer.FetchContainer(type);
        }

        void ITargetContainer.RegisterContainer(Type type, ITargetContainer container)
        {
            DependencyTargetContainer.RegisterContainer(type, container);
        }
    }
}
