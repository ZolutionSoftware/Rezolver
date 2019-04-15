// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An implementation of <see cref="ICompileContext"/> which is specialised for use by an <see cref="IExpressionCompiler"/>.
    /// </summary>
    /// <seealso cref="Rezolver.Compilation.CompileContext" />
    public class ExpressionCompileContext : CompileContext, IExpressionCompileContext
    {
        /// <summary>
        /// The default <see cref="ResolveContext"/> parameter expression used by the expression-based code generators
        /// defined in this library, although it's not used directly for that purpose - instead, the <see cref="ResolveContextParameterExpression"/>
        /// of new contexts is initialised to this if not explicitly provided on  construction and when not being inherited from another
        /// context.
        /// </summary>
        public static ParameterExpression DefaultResolveContextParameterExpression { get; }
            = Expression.Parameter(typeof(ResolveContext), "resolveContext");

        /// <summary>
        /// Gets the parent context.
        /// </summary>
        /// <remarks>Note that this property hides the inherited <see cref="ICompileContext.ParentContext"/> property,
        /// since an <see cref="IExpressionCompileContext"/> can only be a child of another <see cref="IExpressionCompileContext"/>.</remarks>
        public new IExpressionCompileContext ParentContext
        {
            get
            {
                return (IExpressionCompileContext)base.ParentContext;
            }
        }

        //private readonly Expression _currentContainerExpression;
        ///// <summary>
        ///// Gets an expression which gives a reference to the <see cref="IContainer" /> for this context -
        ///// i.e. the one on the <see cref="ICompileContext.ResolveContext" /> property.
        ///// </summary>
        ///// <value>The container expression.</value>
        ///// <remarks>Note that this is *not* the same as <see cref="ContextContainerPropertyExpression" /> - but is provided
        ///// to allow expressions to be compiled which compare the container supplied at compile time to the one from the
        ///// <see cref="ResolveContext.Container" /> at resolve-time.</remarks>
        //public Expression CurrentContainerExpression
        //{
        //    get
        //    {
        //        return this._currentContainerExpression ?? ParentContext.CurrentContainerExpression;
        //    }
        //}

        private readonly ParameterExpression _resolveContextExpression;
        /// <summary>
        /// This is the parameter expression which represents the <see cref="ResolveContext" /> that is passed to the
        /// <see cref="ICompiledTarget" /> at resolve-time.
        /// The other expressions - <see cref="ContextContainerPropertyExpression" /> and <see cref="ContextScopePropertyExpression" />
        /// are both built from this too.
        /// </summary>
        /// <value>The resolve context expression.</value>
        /// <remarks>If the code produced by the <see cref="IExpressionBuilder" /> for a given target needs to read or use the
        /// <see cref="ResolveContext" /> that was originally passed to the <see cref="IContainer.Resolve(ResolveContext)" /> method,
        /// then it does it by using this expression, which will be set as the only parameter on the lambda expression which is
        /// eventually compiled (in the case of the default expression compiler, <see cref="ExpressionCompiler" />.</remarks>
        public ParameterExpression ResolveContextParameterExpression
        {
            get
            {
                return this._resolveContextExpression ?? ParentContext.ResolveContextParameterExpression;
            }
        }

        //private readonly MemberExpression _contextContainerPropertyExpression;
        ///// <summary>
        ///// Gets an expression for reading the <see cref="ResolveContext.Container" /> property of the <see cref="ResolveContext" />
        ///// that's in scope when the <see cref="ICompiledTarget" /> (which is built from the compiled expression) is executed.
        ///// </summary>
        ///// <value>The context container property expression.</value>
        //public MemberExpression ContextContainerPropertyExpression
        //{
        //    get
        //    {
        //        return this._contextContainerPropertyExpression ?? ParentContext.ContextContainerPropertyExpression;
        //    }
        //}

        //private readonly MemberExpression _contextScopePropertyExpression;

        ///// <summary>
        ///// Gets an expression for reading the <see cref="ResolveContext.Scope" /> property of the <see cref="ResolveContext" />
        ///// that's in scope when the <see cref="ICompiledTarget" /> (which is built from the compiled expression) is executed.
        ///// </summary>
        ///// <value>The context scope property expression.</value>
        //public MemberExpression ContextScopePropertyExpression
        //{
        //    get
        //    {
        //        return this._contextScopePropertyExpression ?? ParentContext.ContextScopePropertyExpression;
        //    }
        //}

        private readonly Dictionary<SharedExpressionKey, Expression> _sharedExpressions;

        /// <summary>
        /// Gets a read-only enumerable of all the shared expressions that have been inherited from any parent context and/or added
        /// via calls to <see cref="GetOrAddSharedExpression(Type, string, Func{Expression}, Type)" /> or
        /// <see cref="GetOrAddSharedLocal(Type, string, Type)" />.
        /// </summary>
        public IEnumerable<Expression> SharedExpressions
        {
            get
            {
                return this._sharedExpressions != null ? this._sharedExpressions.Values : ParentContext.SharedExpressions;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileContext"/> class as a child of another.
        ///
        /// Note that all the expression properties (<see cref="ResolveContextParameterExpression"/>, <see cref="ContextContainerPropertyExpression"/>
        /// and <see cref="ContextScopePropertyExpression"/>) are always inherited from the source context to ensure consistency across
        /// all expressions being built during a particular compilation chain.
        /// </summary>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="useParentSharedExpressions">If <c>true</c> then the <see cref="SharedExpressions"/> of the <paramref name="sourceContext"/>
        /// will be reused by this new context.  If <c>false</c>, then this context will start with a new empty set of shared expressions.</param>
        /// <param name="scopeBehaviourOverride">Override the scope behaviour to be used for the target that is compiled with this context.</param>
        /// <param name="targetType">If not null, the type for which expressions are to be compiled.  If null, then the
        /// <paramref name="sourceContext"/>'s <see cref="ICompileContext.TargetType"/> will be inherited.</param>
        /// <param name="scopePreferenceOverride">Allows you to override the scope preference for any target being compiled in this context -
        /// if not provided, then it is automatically inherited from the parent.</param>
        protected internal ExpressionCompileContext(IExpressionCompileContext sourceContext,
            Type targetType = null,
            bool useParentSharedExpressions = true,
            ScopeBehaviour? scopeBehaviourOverride = null,
            ScopePreference? scopePreferenceOverride = null)
            : base(sourceContext, targetType, scopeBehaviourOverride, scopePreferenceOverride)
        {
            this._sharedExpressions = useParentSharedExpressions ? null : new Dictionary<SharedExpressionKey, Expression>();

            RegisterExpressionTargets();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileContext"/> class.
        /// </summary>
        /// <seealso cref="CompileContext.CompileContext(ResolveContext, ITargetContainer, Type)"/>.
        /// <param name="resolveContext">Required.  The container for which the compilation is being performed.  When compiling in response to
        /// a call to <see cref="IContainer.Resolve(ResolveContext)"/>, the container which first receives the call should be
        /// the one passed here.</param>
        /// <param name="dependencyTargetContainer">Required - target container used for dependency lookups.  As with the base class
        /// this is actually wrapped in a new <see cref="OverridingTargetContainer"/> and used as this class' implementation of
        /// <see cref="ITargetContainer"/>.</param>
        /// <param name="targetType">Optional. Will be set into the <see cref="CompileContext.TargetType" /> property.  If null, then any
        /// <see cref="ITarget"/> that is compiled should be compiled for its own <see cref="ITarget.DeclaredType"/>.</param>
        /// <param name="resolveContextExpression">Optional, mapped to <see cref="ResolveContextParameterExpression"/> - the default
        /// for this (i.e. when you leave it as null) is to use the static <see cref="DefaultResolveContextParameterExpression"/>
        /// and generally it should always be left as that.
        ///
        /// The <see cref="ExpressionCompiler"/>, when building expressions to turn into compiled lambdas, uses this as the main parameter
        /// on the lambda itself.</param>
        protected internal ExpressionCompileContext(ResolveContext resolveContext,
          ITargetContainer dependencyTargetContainer,
          Type targetType = null,
          ParameterExpression resolveContextExpression = null)
            : base(resolveContext, dependencyTargetContainer, targetType)
        {
            this._resolveContextExpression = resolveContextExpression ?? DefaultResolveContextParameterExpression;
            this._sharedExpressions = new Dictionary<SharedExpressionKey, Expression>(20);
            //this._currentContainerExpression = Expression.Constant(resolveContext.Container, typeof(Container));
            //this._contextContainerPropertyExpression = Expression.Property(this._resolveContextExpression, nameof(Rezolver.ResolveContext.Container));
            //this._contextScopePropertyExpression = Expression.Property(ResolveContextParameterExpression, nameof(Rezolver.ResolveContext.Scope));
            RegisterExpressionTargets();
        }

        /// <summary>
        /// Registers some additional targets into the compile context to support compilation.
        /// </summary>
        protected void RegisterExpressionTargets()
        {
            //DependencyTargetContainer.Register(new ExpressionTarget(ContextContainerPropertyExpression, typeof(Container)));
        }

        /// <summary>
        /// Creates a new <see cref="IExpressionCompileContext" /> using this one as a seed.  This function is identical to
        /// <see cref="ICompileContext.NewContext(Type, ScopeBehaviour?, ScopePreference?)" /> but allows you to control whether the <see cref="SharedExpressions" />
        /// are inherited (the default); and is more convenient because it returns another <see cref="IExpressionCompileContext" />.
        /// </summary>
        /// <param name="targetType">Optional.  The type for which the target is to be compiled, if different from this
        /// context's <see cref="CompileContext.TargetType" />.</param>
        /// <param name="useParentSharedExpressions">If <c>true</c> then the shared expressions in this context will be inherited
        /// by the new context by reference.  That is, when the new context goes out of scope, any new shared expressions it created
        /// will still be available.
        /// If false, then the new context will get a brand new, empty, set of shared expressions.</param>
        /// <param name="scopeBehaviourOverride">Override the <see cref="CompileContext.ScopeBehaviourOverride"/> to be used for the target that is compiled with the new context.
        /// This is never inherited automatically from one context to another.</param>
        /// <param name="scopePreferenceOverride">Sets the <see cref="CompileContext.ScopePreferenceOverride"/>.  As soon as this is set on one context, it is automatically
        /// inherited by all its child contexts (i.e. you cannot null it)</param>
        IExpressionCompileContext IExpressionCompileContext.NewContext(
            Type targetType,
            bool useParentSharedExpressions,
            ScopeBehaviour? scopeBehaviourOverride,
            ScopePreference? scopePreferenceOverride)
        {
            return new ExpressionCompileContext(this,
                targetType,
                useParentSharedExpressions,
                scopeBehaviourOverride,
                scopePreferenceOverride);
        }

        /// <summary>
        /// Used by the explicit implementation of
        /// <see cref="Rezolver.Compilation.ICompileContext.NewContext(Type, ScopeBehaviour?, ScopePreference?)" />.
        /// This is overriden to ensure that the correct type of context is created when created directly through the
        /// <see cref="ICompileContext"/> interface.
        /// </summary>
        /// <param name="targetType">Optional.  The type for which the target is to be compiled, if different from this
        /// context's <see cref="Rezolver.Compilation.ICompileContext.TargetType" />.</param>
        /// <param name="scopeBehaviourOverride">Override the <see cref="CompileContext.ScopeBehaviourOverride"/> to be used for the target that is compiled with the new context.
        /// This is never inherited automatically from one context to another.</param>
        /// <param name="scopePreferenceOverride">Sets the <see cref="CompileContext.ScopePreferenceOverride"/>.  As soon as this is set on one context, it is automatically
        /// inherited by all its child contexts (i.e. you cannot null it)</param>
        /// <remarks>Note all child contexts created through this virtual method will always inherit the parent context's shared expressions.</remarks>
        protected override ICompileContext NewContext(
            Type targetType = null,
            ScopeBehaviour? scopeBehaviourOverride = null,
            ScopePreference? scopePreferenceOverride = null)
        {
            return ((IExpressionCompileContext)this).NewContext(targetType,
                scopeBehaviourOverride: scopeBehaviourOverride,
                scopePreferenceOverride: scopePreferenceOverride);
        }

        /// <summary>
        /// Similar to <see cref="GetOrAddSharedExpression(Type, string, Func{Expression}, Type)" />, except this is used when expression
        /// builders want to use local variables in block expressions to store the result of some operation in the expression tree built
        /// for a particular target.  Reusing one local variable is more efficient than declaring the same local multiple times.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="requestingType">Optional - the type of the object requesting this shared expression.  If this is provided,
        /// then the search for an existing shared expression will only work if the same requesting type was passed previously.</param>
        /// <exception cref="InvalidOperationException">Cannot add ParameterExpression: A shared expression of a different type has already been added with the same type and name.</exception>
        /// <remarks>When multiple expression trees from multiple targets are brought together into one lambda, there will
        /// often be many duplicate variables which could be shared.  So, if an <see cref="IExpressionBuilder" /> needs a local variable
        /// for a block, instead of simply declaring it directly through the <see cref="Expression.Parameter(Type, string)" /> function,
        /// it can use this function instead, which will return a previously created one if available.</remarks>
        public ParameterExpression GetOrAddSharedLocal(Type type, string name, Type requestingType = null)
        {
            // if no sharedExpressions dictionary, then we have a parent which will handle the call for us.
            if (this._sharedExpressions == null)
            {
                return ParentContext.GetOrAddSharedLocal(type, name, requestingType);
            }

            try
            {
                return (ParameterExpression)GetOrAddSharedExpression(type, name, (t, s) => Expression.Parameter(t, s), requestingType);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException("Cannot add ParameterExpression: A shared expression of a different type has already been added with the same type and name.");
            }
        }

        /// <summary>
        /// Gets or adds a shared expression (created by the <paramref name="expressionFactory"/> if it's not already cached) with the given name, type, optionally
        /// for the given <paramref name="requestingType"/>.
        /// </summary>
        /// <param name="type">The runtime type of the Expression.</param>
        /// <param name="name">The runtime name of the Expression - and also the name used to retrieve it later.</param>
        /// <param name="expressionFactory">The factory method to use to construct the shared expression from scratch, if it's not already cached.</param>
        /// <param name="requestingType">Optional - to avoid naming clashes with shared expressions created by other targets, you can pass a type here
        /// (usually the runtime type of your <see cref="ITarget"/> implementation).</param>
        /// <returns>Expression.</returns>
        public Expression GetOrAddSharedExpression(Type type, string name, Func<Type, string, Expression> expressionFactory, Type requestingType = null)
        {
            if (this._sharedExpressions == null)
            {
                return ParentContext.GetOrAddSharedExpression(type, name, expressionFactory, requestingType);
            }

            if(type == null) throw new ArgumentNullException(nameof(type));
            if(expressionFactory == null) throw new ArgumentNullException(nameof(expressionFactory));
            // if this is
            SharedExpressionKey key = new SharedExpressionKey(type, name, requestingType);
            if (!this._sharedExpressions.TryGetValue(key, out Expression toReturn))
            {
                this._sharedExpressions[key] = toReturn = expressionFactory(type, name);
            }

            return toReturn;
        }
    }
}
