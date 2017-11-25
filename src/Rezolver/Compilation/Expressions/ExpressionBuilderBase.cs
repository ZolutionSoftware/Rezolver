// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Abstract starting point for implementing <see cref="IExpressionBuilder"/>.
    /// 
    /// Note that the interface is implemented explicitly; but exposes protected abstract or virtual 
    /// methods for inheritors to extend.
    /// </summary>
    /// <seealso cref="Rezolver.Compilation.Expressions.IExpressionBuilder" />
    /// <remarks>This class takes care of checking the type requested in the <see cref="IExpressionCompileContext"/>
    /// is compatible with the target that's passed to the <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>
    /// method</remarks>
    public abstract class ExpressionBuilderBase : IExpressionBuilder
    {
        /// <summary>
        /// Provides access to a set of <see cref="MethodInfo"/> objects for common functions required
        /// by code produced from <see cref="ITarget"/>s.  Also contains some factory methods for building
        /// expressions which bind to those methods.
        /// </summary>
        protected static class Methods
        {
            /// <summary>
            /// Gets a <see cref="MethodInfo"/> for the <see cref="IDirectTarget.GetValue"/> method.
            /// </summary>
            public static MethodInfo IDirectTarget_GetValue_Method =>
            Extract.Method((IDirectTarget t) => t.GetValue());

            /// <summary>
            /// Gets a <see cref="MethodInfo"/> for the <see cref="ICompiledTarget.GetObject(IResolveContext)"/>
            /// method.
            /// </summary>
            public static MethodInfo ICompiledTarget_GetObject_Method =>
                Extract.Method((ICompiledTarget t) => t.GetObject(null));

            /// <summary>
            /// Gets a <see cref="MethodInfo"/> for the <see cref="ContainerScopeExtensions.GetRootScope(IContainerScope)"/>
            /// extension method.
            /// </summary>
            public static MethodInfo IContainerScope_GetRootScope_Method =>
                Extract.Method((IContainerScope s) => s.GetRootScope());

            /// <summary>
            /// Get a <see cref="MethodInfo"/> for the <see cref="IContainer.CanResolve(IResolveContext)"/>
            /// method
            /// </summary>
            public static MethodInfo IContainer_CanResolve_Method =>
                Extract.Method(
                    (IContainer c) => c.CanResolve((IResolveContext)null));

            /// <summary>
            /// Gets a <see cref="MethodInfo"/> for the <see cref="IContainer.Resolve(IResolveContext)"/> method
            /// </summary>
            public static MethodInfo IContainer_Resolve_Method =>
                Extract.Method((IContainer c) => c.Resolve(null));

            /// <summary>
            /// Gets a MethodInfo object for the <see cref="IContainerScope.Resolve(IResolveContext, ITarget, Func{IResolveContext, object}, ScopeBehaviour)"/>
            /// method for help in generating scope-interfacing code.
            /// </summary>
            public static MethodInfo IContainerScope_Resolve_Method =>
                Extract.Method(
                    (IContainerScope s) => s.Resolve(
                        (IResolveContext)null,
                        Guid.Empty,
                        (Func<IResolveContext, object>)null,
                        ScopeBehaviour.None));

            /// <summary>
            /// Gets a <see cref="MethodInfo"/> for the <see cref="ResolveContextExtensions.Resolve(IResolveContext, ITarget, Func{IResolveContext, object}, ScopeBehaviour)"/>
            /// extension method.
            /// </summary>
            public static MethodInfo ResolveContextExtensions_Resolve_Method =>
                Extract.Method(
                    (IResolveContext rc) => rc.Resolve(Guid.Empty,
                        (Func<IResolveContext, object>)null,
                        ScopeBehaviour.None));


            /// <summary>
            /// Gets a MethodInfo object for the <see cref="IResolveContext.New(Type, IContainer, IContainerScope)"/> method
            /// </summary>
            /// <value>The type of the resolve context create new method.</value>
            public static MethodInfo IResolveContext_New_Method =>
                Extract.Method((IResolveContext r) => r.New(null, null, null));

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents calling the
            /// <see cref="IResolveContext.New(Type, IContainer, IContainerScope)"/> method with the
            /// given arguments.
            /// </summary>
            /// <param name="resolveContext">An expression representing the context on which the method will be called</param>
            /// <param name="newRequestedType">An expression representing the argument to the newRequestedType parameter</param>
            /// <param name="newContainer">An expression representing the argument to the newContainer parameter</param>
            /// <param name="newScope">An expression representing the argument to the newScope parameter</param>
            /// <returns></returns>
            public static MethodCallExpression CallResolveContext_New(Expression resolveContext,
                Expression newRequestedType = null,
                Expression newContainer = null,
                Expression newScope = null)
            {
                return Expression.Call(resolveContext,
                    IResolveContext_New_Method,
                    newRequestedType ?? Expression.Default(typeof(Type)),
                    newContainer ?? Expression.Default(typeof(IContainer)),
                    newScope ?? Expression.Default(typeof(IContainerScope)));
            }

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents calling the
            /// <see cref="IContainer.Resolve(IResolveContext)"/> method with the given
            /// context argument.
            /// </summary>
            /// <param name="container">An expression representing the container on which the method will be called</param>
            /// <param name="context">An expression representing the argument to the context parameter</param>
            /// <returns></returns>
            public static MethodCallExpression CallIContainer_Resolve(Expression container,
                Expression context)
            {
                return Expression.Call(container, IContainer_Resolve_Method, context);
            }

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents calling the
            /// <see cref="IContainer.CanResolve(IResolveContext)"/> method with the given
            /// context argument.
            /// </summary>
            /// <param name="container">An expression representing the container on which the method will be called</param>
            /// <param name="context">An expression representing the argument to the context parameter</param>
            /// <returns></returns>
            public static MethodCallExpression CallIContainer_CanResolve(Expression container,
                Expression context)
            {
                return Expression.Call(container, IContainer_CanResolve_Method, context);
            }
        }

        /// <summary>
        /// Gets the <see cref="IExpressionCompiler"/> to be used for
        /// the given context, if different from one passed to this class' implementation of 
        /// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>.
        /// </summary>
        /// <param name="context">The current compile context.</param>
        /// <remarks>This function is called by <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)"/> which will throw
        /// an exception if it returns null and no compiler was provided to <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// (typically via the explicit implementation of <see cref="IExpressionBuilder"/>).
        /// 
        /// The base implementation simply attempts to resolve an instance of <see cref="IExpressionCompiler"/>
        /// from the <see cref="IResolveContext.Container"/> of the context's <see cref="ICompileContext.ResolveContext"/> which should, 
        /// with the default configuration, resolve to the root <see cref="ExpressionCompiler"/>.  In order for this to work, it is 
        /// imperative that the underlying registered target implements the ICompiledTarget interface - so as to avoid needing a (or, 
        /// more precisely, this) compiler needing to compile it.</remarks>
        protected virtual IExpressionCompiler GetContextCompiler(IExpressionCompileContext context)
        {
            return context.GetOption<IExpressionCompiler>();
        }

        /// <summary>
        /// The core expression build function - takes care of handling mismatched types between the target and
        /// the requested type in the context - both checking compatibility and producing conversion expressions
        /// where necessary.
        /// Also performs cyclic dependency checking and rewriting expressions to take advantage of a target's
        /// <see cref="ITarget.ScopeBehaviour"/> (which can be overriden with <see cref="ICompileContext.ScopeBehaviourOverride"/>)
        /// </summary>
        /// <param name="target">The target to be compiled.</param>
        /// <param name="context">The context.</param>
        /// <param name="compiler">The compiler.</param>
        /// <exception cref="ArgumentException">targetType doesn't support the context's <see cref="ICompileContext.TargetType"/></exception>
        /// <exception cref="InvalidOperationException">The <paramref name="target"/> is already being compiled.</exception>
        /// <remarks>This class' implementation of <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> calls this,
        /// as does the derived abstract class <see cref="ExpressionBuilderBase{TTarget}" /> for its implementation
        /// of <see cref="IExpressionBuilder{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)" />.
        /// 
        /// 
        /// It is this function that is responsible for calling the abstract <see cref="Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" />
        /// function, which deriving classes implement to actually produce their expression for the <paramref name="target"/>.
        /// </remarks>
        protected internal Expression BuildCore(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            if (context.TargetType != null && !target.SupportsType(context.TargetType))
                throw new ArgumentException(String.Format(ExceptionResources.TargetDoesntSupportType_Format, context.TargetType),
                  "targetType");

            if (!context.PushCompileStack(target))
                throw new InvalidOperationException(string.Format(ExceptionResources.CyclicDependencyDetectedInTargetFormat, target.GetType(), target.DeclaredType));

            try
            {
                var result = Build(target, context, compiler);
                Type convertType = context.TargetType ?? target.DeclaredType;

                //expands targets which have baked into expressions
                result = new TargetExpressionRewriter(compiler, context).Visit(result);

                //always convert if types aren't the same.
                if (convertType != result.Type)
                    result = Expression.Convert(result, convertType);

                result = ApplyScoping(result, target, context, compiler);

                return result;
            }
            finally
            {
                context.PopCompileStack();
            }
        }

        /// <summary>
        /// Called by the <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)"/> method.
        /// 
        /// Applies the scoping behaviour to the <paramref name="builtExpression"/> such that when it is executed it 
        /// correctly interfaces with the active scope (from the <see cref="IResolveContext"/>) if one is present for the
        /// scope behaviour and preference determined from the <paramref name="context"/> and <paramref name="target"/>.
        /// </summary>
        /// <param name="builtExpression">The expression that was built for the <paramref name="target"/>.</param>
        /// <param name="target">The target that is currently being compiled and from which the <paramref name="builtExpression" />
        /// was built.</param>
        /// <param name="context">The current active compilation context - this context's <see cref="ICompileContext.ScopeBehaviourOverride"/>
        /// and <see cref="ICompileContext.ScopePreferenceOverride"/> will be used to override those of the <paramref name="target"/>.</param>
        /// <param name="compiler">The compiler.</param>
        protected virtual Expression ApplyScoping(Expression builtExpression, ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var scopeBehaviour = context.ScopeBehaviourOverride ?? target.ScopeBehaviour;
            var scopePreference = context.ScopePreferenceOverride ?? target.ScopePreference;

            if (scopeBehaviour == ScopeBehaviour.None)
                return builtExpression;

            var originalType = builtExpression.Type;
            //so the expression needs to be rewritten so that it becomes something like this:
            //if(context.Scope != null) return context.Scope.Resolve(context, lambda<targetExpression>);
            //else return targetExpression

            //TODO: if behaviour == explicit, then a scope is *required* and the expression should throw
            //an exception if it is null :)

            //this will automatically be of type object and will be optimised.
            var lambda = compiler.BuildResolveLambda(builtExpression, context).Compile();
            //use a shared expression for the scope check so we can optimise away all the nested scope calls
            //we're likely to be generating.
            var compareExpr = context.GetOrAddSharedExpression(typeof(bool), "isScoped", () =>
            {
                return Expression.Equal(context.ContextScopePropertyExpression, Expression.Default(typeof(IContainerScope)));
            }, typeof(ExpressionBuilderBase));
            
            // have to force the creation of a new IResolveContext whose RequestedType type is equal to the type
            // that we sought for compilation - so that the instance can be tracked correctly.
            var newContextExpr = Methods.CallResolveContext_New(
                    context.ResolveContextParameterExpression,
                    Expression.Constant(builtExpression.Type),
                            Expression.Default(typeof(IContainer)),
                            scopePreference == ScopePreference.Current ? (Expression)Expression.Default(typeof(IContainerScope)) : 
                            Expression.Call(Methods.IContainerScope_GetRootScope_Method,
                                context.ContextScopePropertyExpression)
                    );

            var @override = context.GetOption<Runtime.TargetIdentityOverride>(context.TargetType ?? target.DeclaredType) ;

            builtExpression = Expression.Condition(
                compareExpr,
                builtExpression, //if null scope, just use the built expression as-is
                Expression.Convert( //otherwise - generate a call into the scope's special Resolve method
                    Expression.Call(
                        Methods.ResolveContextExtensions_Resolve_Method,
                        newContextExpr,
                        Expression.Constant(@override ?? target.Id),
                        Expression.Constant(lambda),
                        Expression.Constant(scopeBehaviour)
                    ),
                    originalType
                ));

            return builtExpression;
        }

        /// <summary>
        /// Explicit implementation of <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> -
        /// ultimately forwards the call to the <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> function.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">Optional. The compiler that's requesting the expression; and which can be used
        /// to compile other targets within the target.  If not provided, then the implementation attempts to locate
        /// the context compiler using the <see cref="GetContextCompiler(IExpressionCompileContext)"/> method, and will throw
        /// an <see cref="InvalidOperationException"/> if it cannot do so.</param>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null or <paramref name="context"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="compiler"/> is null and an IExpressionCompiler 
        /// couldn't be resolved for the current context (via <see cref="GetContextCompiler(IExpressionCompileContext)"/></exception>
        Expression IExpressionBuilder.Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            target.MustNotBeNull(nameof(target));
            context.MustNotBeNull(nameof(context));

            if (compiler == null)
            {
                compiler = GetContextCompiler(context);
                if (compiler == null)
                    throw new InvalidOperationException("Unable to identify the IExpressionCompiler for the current context");
            }

            return BuildCore(target, context, compiler);
        }

        /// <summary>
        /// Abstract method used as part implementation of the
        /// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" />
        /// It's called by <see cref="BuildCore(ITarget, IExpressionCompileContext, IExpressionCompiler)" />.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target"/>.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided </param>
        protected abstract Expression Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler);

        /// <summary>
        /// Abstract method (implementation of <see cref="IExpressionBuilder.CanBuild(ITarget)"/>) which
        /// determines whether this instance can build an expression for the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        public abstract bool CanBuild(ITarget target);
    }
}
