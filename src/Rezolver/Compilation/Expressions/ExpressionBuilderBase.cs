// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        private static readonly ConcurrentDictionary<int, ITarget> _compiledTargets = new ConcurrentDictionary<int, ITarget>();
        private static readonly ConcurrentDictionary<TypeAndTargetId, int> _compileCounts = new ConcurrentDictionary<TypeAndTargetId, int>();

        private static void TrackCompilation(ITarget target, IExpressionCompileContext context)
        {
            Type theType = context.TargetType ?? target.DeclaredType;
            int? targetIdOverride = context.GetOption<Runtime.TargetIdentityOverride>(theType);

            _compiledTargets.GetOrAdd(targetIdOverride ?? target.Id, target);
            _compileCounts.AddOrUpdate(new TypeAndTargetId(theType, targetIdOverride ?? target.Id), 1, (k, i) => i + 1);
        }

        public static IEnumerable<(Type type, int id, ITarget target, int count)> GetCompileCounts()
        {
            return _compileCounts.Skip(0).Select(kvp => (kvp.Key.Type, kvp.Key.Id, _compiledTargets[kvp.Key.Id], kvp.Value));
        }

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
            /// Gets a <see cref="MethodInfo"/> for the <see cref="IInstanceProvider.GetInstance(ResolveContext)"/>
            /// method.
            /// </summary>
            public static MethodInfo IInstanceProvider_GetInstance_Method =>
                Extract.Method((IInstanceProvider ip) => ip.GetInstance(default));

            public static MethodInfo Container_ResolveStrong_Method =>
                Extract.Method((Container c) => c.ResolveInternal<object>(default)).GetGenericMethodDefinition();

            public static MethodInfo CurrentScope_ActivateImplicit_Method =>
                Extract.Method((ResolveContext c) => c.ActivateImplicit_ThisScope((object)null)).GetGenericMethodDefinition();

            public static MethodInfo RootScope_ActivateImplicit_Method =>
                Extract.Method((ResolveContext c) => c.ActivateImplicit_RootScope((object)null)).GetGenericMethodDefinition();

            public static MethodInfo CurrentScope_ActivateExplicit_Method =>
                Extract.Method((ResolveContext c) => c.ActivateExplicit_ThisScope<object>(0, null)).GetGenericMethodDefinition();

            public static MethodInfo RootScope_ActivateExplicit_Method =>
                Extract.Method((ResolveContext c) => c.ActivateExplicit_RootScope<object>(0, null)).GetGenericMethodDefinition();

            /// <summary>
            /// Gets a MethodInfo object for the <see cref="ResolveContext.ChangeRequestedType(Type)"/> method
            /// </summary>
            /// <value>The type of the resolve context create new method.</value>
            public static MethodInfo ResolveContext_New_Type_Method =>
                Extract.Method((ResolveContext r) => r.ChangeRequestedType((Type)null));

            /// <summary>
            /// Gets a MethodInfo object for the <see cref="ResolveContext.ChangeContainer(Container)"/> method
            /// </summary>
            /// <value>The type of the resolve context create new method.</value>
            public static MethodInfo ResolveContext_New_Container_Method =>
                Extract.Method((ResolveContext r) => r.ChangeContainer((Container)null));

            public static MethodInfo ResolveContext_Resolve_Strong_Method =>
                Extract.Method((ResolveContext r) => r.Resolve<object>()).GetGenericMethodDefinition();

            public static MethodCallExpression Call_CurrentScope_ActivateImplicit(
                Expression context,
                Expression instance)
            {
                return Expression.Call(
                    context,
                    CurrentScope_ActivateImplicit_Method.MakeGenericMethod(instance.Type),
                    instance);
            }

            public static MethodCallExpression Call_RootScope_ActivateImplicit(
                Expression context,
                Expression instance)
            {
                return Expression.Call(
                    context,
                    RootScope_ActivateImplicit_Method.MakeGenericMethod(instance.Type),
                    instance);
            }

            public static MethodCallExpression Call_CurrentScope_ActivateExplicit(
                Expression context,
                int targetId,
                LambdaExpression instanceFactory)
            {
                return Expression.Call(
                    context,
                    CurrentScope_ActivateExplicit_Method.MakeGenericMethod(instanceFactory.Body.Type),
                    Expression.Constant(targetId),
                    instanceFactory);
            }

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents activating the instance produced by the 
            /// <paramref name="instanceFactory"/> through the root scope.  Only to be used when compiling a target 
            /// which has a <see cref="ITarget.ScopeBehaviour"/> of <see cref="ScopeBehaviour.Explicit"/> and a
            /// <see cref="ITarget.ScopePreference"/> of <see cref="ScopePreference.Root" />.
            /// </summary>
            /// <param name="context">An expression representing the context on which the method will be called</param>
            /// <param name="targetId">The <see cref="ITarget.Id"/> of the target from which the <paramref name="instanceFactory"/> was compiled.</param>
            /// <param name="instanceFactory">A lambda expression </param>
            /// <returns></returns>
            public static MethodCallExpression Call_RootScope_ActivateExplicit(
                Expression context,
                int targetId,
                LambdaExpression instanceFactory)
            {
                return Expression.Call(
                    context,
                    RootScope_ActivateExplicit_Method.MakeGenericMethod(instanceFactory.Body.Type),
                    Expression.Constant(targetId),
                    instanceFactory);
            }

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents calling the
            /// <see cref="ResolveContext.ChangeRequestedType(Type)"/> method with the
            /// given arguments.
            /// </summary>
            /// <param name="resolveContext">An expression representing the context on which the method will be called</param>
            /// <param name="serviceType">An expression representing the argument to the newRequestedType parameter</param>
            /// <returns>A <see cref="MethodCallExpression"/></returns>
            public static MethodCallExpression CallResolveContext_New_Type(Expression resolveContext,
                Expression serviceType)
            {
                return Expression.Call(resolveContext,
                    ResolveContext_New_Type_Method,
                    serviceType);
            }

            /// <summary>
            /// Emits a <see cref="MethodCallExpression"/> which represents calling the
            /// <see cref="ResolveContext.Resolve{TService}()"/> method of the context represented
            /// by the <paramref name="resolveContext"/> expression.
            /// </summary>
            /// <param name="resolveContext">Expression representing the context on which the method is to be called.</param>
            /// <param name="serviceType">A type which will be used to bind the correct generic method.</param>
            /// <returns>A <see cref="MethodCallExpression"/></returns>
            public static MethodCallExpression CallResolveContext_Resolve_Strong_Method(
                Expression resolveContext,
                Type serviceType)
            {
                return Expression.Call(
                    resolveContext,
                    ResolveContext_Resolve_Strong_Method.MakeGenericMethod(serviceType));
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
        /// from the <see cref="ResolveContext.Container"/> of the context's <see cref="ICompileContext.ResolveContext"/> which should,
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
            {
                throw new InvalidOperationException(string.Format(ExceptionResources.CyclicDependencyDetectedInTargetFormat, target.GetType(), target.DeclaredType));
            }

            try
            {
                var result = Build(target, context, compiler);
                Type convertType = context.TargetType ?? target.DeclaredType;

                // expands targets which have baked into expressions
                result = new TargetExpressionRewriter(compiler, context).Visit(result);

                // always convert if types aren't the same.
                if (convertType != result.Type)
                {
                    result = Expression.Convert(result, convertType);
                }

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
        /// correctly interfaces with the active scope (from the <see cref="ResolveContext"/>) if one is present for the
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

            if (scopeBehaviour == ScopeBehaviour.None)
            {
                return builtExpression;
            }

            var scopePreference = context.ScopePreferenceOverride ?? target.ScopePreference;
            
            // this will now call the correct activation method (internal-only) on the ResolveContext
            if (scopeBehaviour == ScopeBehaviour.Implicit)
            {
                if (scopePreference == ScopePreference.Current)
                {
                    return Methods.Call_CurrentScope_ActivateImplicit(
                        context.ResolveContextParameterExpression,
                        builtExpression);
                }
                else
                {
                    return Methods.Call_RootScope_ActivateImplicit(
                        context.ResolveContextParameterExpression,
                        builtExpression);
                }
            }
            else
            {
                int? targetIdOverride = context.GetOption<Runtime.TargetIdentityOverride>(context.TargetType ?? target.DeclaredType);

                if (scopePreference == ScopePreference.Current)
                {
                    return Methods.Call_CurrentScope_ActivateExplicit(
                        context.ResolveContextParameterExpression,
                        targetIdOverride ?? target.Id,
                        compiler.BuildResolveLambdaStrong(builtExpression, context));
                }
                else
                {
                    return Methods.Call_RootScope_ActivateExplicit(
                        context.ResolveContextParameterExpression,
                        targetIdOverride ?? target.Id,
                        compiler.BuildResolveLambdaStrong(builtExpression, context));
                }
            }
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
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (compiler == null)
            {
                compiler = GetContextCompiler(context);
                if (compiler == null)
                {
                    throw new InvalidOperationException("Unable to identify the IExpressionCompiler for the current context");
                }
            }
            TrackCompilation(target, context);
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
        /// Abstract method (implementation of <see cref="IExpressionBuilder.CanBuild(Type)"/>) which
        /// determines whether this instance can build an expression for the specified target.
        /// </summary>
        /// <param name="targetType">The type of target.</param>
        public abstract bool CanBuild(Type targetType);
    }
}
