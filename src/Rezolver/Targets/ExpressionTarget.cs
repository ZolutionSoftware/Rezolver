// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
    /// <summary>
    /// A generic target for all expressions not explicitly supported by a particular target.
    ///
    /// Enables more complex behaviours to be registered and used with the more formal
    /// <see cref="ITarget"/> implementations.
    /// </summary>
    /// <remarks>Note to compiler implementers: This class can be used to represent simple
    /// expressions such as constants, constructor calls and so on; but can also contain whole
    /// lambda expressions with parameters.
    ///
    /// In the latter case, expression parameters are expected to receive injected arguments and,
    /// therefore, some rewriting of the expression is likely to be required.</remarks>
    public partial class ExpressionTarget : TargetBase
    {
        /// <summary>
        /// Gets the static expression represented by this target - if <c>null</c>, then
        /// a factory is being used to produce the expression, which is available from
        /// the <see cref="ExpressionFactory"/> property.
        /// </summary>
        public Expression Expression { get; }

        private readonly ScopeBehaviour _scopeBehaviour;

        /// <summary>
        /// Overrides <see cref="TargetBase.ScopeBehaviour"/> to return the value that's passed on construction.
        /// constructor.
        /// </summary>
        public override ScopeBehaviour ScopeBehaviour => _scopeBehaviour;

        private readonly ScopePreference _scopePreference;

        /// <summary>
        /// Overrides <see cref="TargetBase.ScopePreference"/> to return the value that's passed on construction.
        /// constructor.
        /// </summary>
        public override ScopePreference ScopePreference => _scopePreference;

        /// <summary>
        /// Gets the type of <see cref="Expression"/> or the type that all expressions returned by the
        /// <see cref="ExpressionFactory"/> are expected to be equal to.
        /// </summary>
        public override Type DeclaredType
        {
            get;
        }

        /// <summary>
        /// Gets a factory which will be executed to obtain an expression given a particular <see cref="ICompileContext"/>.
        ///
        /// If <c>null</c>, then a static expression will be used instead and is available
        /// from the <see cref="Expression"/> property.
        /// </summary>
        public Func<ICompileContext, Expression> ExpressionFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTarget" /> class.
        /// </summary>
        /// <param name="expression">Required. The static expression which should be used by compilers.</param>
        /// <param name="declaredType">Declared type of the target to be created (used when registering without
        /// an explicit type or when this target is used as a value inside another target).</param>
        /// <param name="scopeBehaviour">Scope behaviour for this expression.  The default is <see cref="ScopeBehaviour.None"/>, which means
        /// that no disposal will take place by default for the instance.  If the expression produces a new instance, then 
        /// <see cref="ScopeBehaviour.Implicit"/> or <see cref="ScopeBehaviour.Explicit"/> can be used safely - the choice being whether
        /// the expression should produce one instance per scope, or should act as a disposable transient object.</param>
        /// <param name="scopePreference">If <paramref name="scopeBehaviour"/> is not <see cref="ScopeBehaviour.None"/>, then this controls
        /// the preferred scope for the instance to be tracked.  Defaults to <see cref="ScopePreference.Current"/></param>
        /// <remarks><paramref name="declaredType"/> will automatically be determined if not provided
        /// by examining the type of the <paramref name="expression"/>.  For lambdas, the type will
        /// be derived from the Type of the lambda's body.  For all other expressions, the type is
        /// taken directly from the Type property of the expression itself.</remarks>
        public ExpressionTarget(
            Expression expression, 
            Type declaredType = null,
            ScopeBehaviour scopeBehaviour = ScopeBehaviour.None,
            ScopePreference scopePreference = ScopePreference.Current)
        {
            if(expression == null) throw new ArgumentNullException(nameof(expression));
            Expression = expression;
            var expressionType = (expression.NodeType == ExpressionType.Lambda ? ((LambdaExpression)expression).Body.Type : expression.Type);
            DeclaredType = declaredType ?? expressionType;

            if (!DeclaredType.IsAssignableFrom(expressionType))
            {
                throw new ArgumentException($"{nameof(declaredType)} must be compatible with the type of the expression", nameof(declaredType));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTarget"/> class.
        /// </summary>
        /// <param name="expressionFactory">Required. The factory delegate that a compiler should call to get the expression to use when
        /// compiling this target.</param>
        /// <param name="declaredType">Required. Static type of all expressions that will be
        /// returned by <paramref name="expressionFactory"/>.</param>
        /// <param name="scopeBehaviour">Scope behaviour for this expression.  The default is <see cref="ScopeBehaviour.None"/>, which means
        /// that no disposal will take place by default for the instance.  If the expression produces a new instance, then 
        /// <see cref="ScopeBehaviour.Implicit"/> or <see cref="ScopeBehaviour.Explicit"/> can be used safely - the choice being whether
        /// the expression should produce one instance per scope, or should act as a disposable transient object.</param>
        /// <param name="scopePreference">If <paramref name="scopeBehaviour"/> is not <see cref="ScopeBehaviour.None"/>, then this controls
        /// the preferred scope for the instance to be tracked.  Defaults to <see cref="ScopePreference.Current"/></param>
        public ExpressionTarget(
            Func<ICompileContext, Expression> expressionFactory, 
            Type declaredType,
            ScopeBehaviour scopeBehaviour = ScopeBehaviour.None,
            ScopePreference scopePreference = ScopePreference.Current)
        {
            if(expressionFactory == null) throw new ArgumentNullException(nameof(expressionFactory));
            if(declaredType == null) throw new ArgumentNullException(nameof(declaredType));
            ExpressionFactory = expressionFactory;
            DeclaredType = declaredType;
            _scopeBehaviour = scopeBehaviour;
            _scopePreference = scopePreference;
        }

        // TODO: Consider adding a Bind() function to this class to carry out the complex lambda rewriting etc, as it's
        // going to be needed by other compilers, not just the ExpressionCompiler.
    }
}