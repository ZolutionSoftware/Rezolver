// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{

    /// <summary>
    /// Implementation of the <see cref="ITargetCompiler" /> interface which produces factory delegates
    /// by building and compiling expression trees from the <see cref="ITarget" /> objects which are registered.
    /// </summary>
    /// <seealso cref="Rezolver.Compilation.Expressions.IExpressionCompiler" />
    /// <seealso cref="Rezolver.Compilation.ITargetCompiler" />
    /// <remarks>Use of this compiler by a container is enabled by applying the <see cref="Configuration.ExpressionCompilation"/>
    /// <see cref="Configuration.ExpressionCompilation.Instance"/> to the container either on construction (for example, via the
    /// <see cref="Container.Container(IRootTargetContainer, IContainerConfig)"/> constructor) or via the
    /// <see cref="Container.DefaultConfig"/> - which currently configures this compiler by default anyway.
    ///
    /// This class works by directly resolving <see cref="IExpressionBuilder" /> instances which can build an expression for a
    /// given <see cref="ITarget" /> from the <see cref="IExpressionCompileContext" />.
    ///
    /// Typically, this is done by searching for an <see cref="IExpressionBuilder{TTarget}" /> where 'TTarget' is equal to the runtime type
    /// of the target - e.g. <see cref="Targets.ConstructorTarget" />.  If one cannot be found, it will then search for an <see cref="IExpressionBuilder" />
    /// whose <see cref="IExpressionBuilder.CanBuild(Type)" /> function returns <c>true</c> for the given target type.
    ///
    /// With a correctly configured target container this should resolve to an instance of the <see cref="ConstructorTargetBuilder" />
    /// class, which implements <c>IExpressionBuilder&lt;ConstructorTarget&gt;</c>.
    ///
    /// As such, the compiler can be extended to support extra target types and its existing expression builders can be replaced for customised
    /// behaviour because they are all resolved from the <see cref="ITargetContainer" /> underpinning a particular <see cref="CompileContext" />.
    ///
    /// There is a caveat for this, however: you *cannot* use the traditional targets (<see cref="Targets.ConstructorTarget" /> etc) to extend
    /// the compiler because they need to be compiled in order to work - which would cause an infinite recursion.
    ///
    /// As a result, all expression compilers are registered as options through the <see cref="TargetContainerExtensions.SetOption{TOption, TService}(ITargetContainer, TOption)"/>
    /// method, or its non-generic equivalent, with the service type being set to be equal to the type of target that the compiler is for.
    ///
    /// Using this pattern, it's important that an expression builder is completely threadsafe and recursion safe (since one target's
    /// compilation might depend on the compilation of another of the same type).
    /// </remarks>
    public class ExpressionCompiler : IExpressionCompiler, ITargetCompiler
    {
        /// <summary>
        /// Gets the default expression compiler.  It's this that the <see cref="Configuration.ExpressionCompilation"/>
        /// registers.
        /// </summary>
        public static ExpressionCompiler Default { get; } = new ExpressionCompiler();

        /// <summary>
        /// Create the factory for the given <paramref name="target" /> using the <paramref name="context" />
        /// to inform the type of object that is to be built, and for compile-time dependency resolution.
        /// </summary>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="context">Required.  The current compilation context.</param>
        /// <exception cref="System.ArgumentException">context must be an instance of IExpressionCompileContext</exception>
        public Func<ResolveContext, object> CompileTarget(ITarget target, ICompileContext context)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (target is IFactoryProvider factoryProvider)
            {
                return factoryProvider.Factory;
            }
            else if(target is IInstanceProvider instanceProvider)
            {
                return instanceProvider.GetInstance;
            }

            if (context is IExpressionCompileContext exprContext)
            {
                return BuildFactory(target, this.BuildResolveLambda(target, exprContext));
            }
            else
            {
                throw new ArgumentException("context must be an instance of IExpressionCompileContext", nameof(context));
            }
        }

        /// <summary>
        /// Generic, strongly-typed, version of <see cref="CompileTarget(ITarget, ICompileContext)"/> which builds
        /// a strongly-typed factory for the given <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="context">Required.  The current compilation context.</param>
        /// <returns>The compiled delegate.</returns>
        public Func<ResolveContext, TService> CompileTarget<TService>(ITarget target, ICompileContext context)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (target is IFactoryProvider<TService> factoryProvider)
            {
                return factoryProvider.Factory;
            }
            else if (target is IInstanceProvider<TService> instanceProvider)
            {
                return instanceProvider.GetInstance;
            }

            if (context is IExpressionCompileContext exprContext)
            {
                return BuildFactory<TService>(target, this.BuildResolveLambdaStrong(target, exprContext));
            }
            else
            {
                throw new ArgumentException("context must be an instance of IExpressionCompileContext", nameof(context));
            }
        }

        /// <summary>
        /// Resolves an expression builder that can build the given target for the given compile context.
        ///
        /// Or
        ///
        /// Returns null if no builder can be found.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <remarks>The function builds a list of all the types in the hierarchy represented
        /// by the type of the <paramref name="target"/> and, for each of those types which are
        /// compatible with <see cref="ITarget"/>, it looks for an <see cref="IExpressionBuilder{TTarget}"/>
        /// which is specialised for that type.  If no compatible builder is found, then it attempts
        /// to find a general purpose <see cref="IExpressionBuilder"/> which can build the type.</remarks>
        public virtual IExpressionBuilder ResolveBuilder(ITarget target, IExpressionCompileContext context)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.GetOption<IExpressionBuilder>(target.GetType());
        }

        /// <summary>
        /// Creates a factory delegate from the finalised <paramref name="lambda"/> expression which was
        /// previously built for a target.
        /// </summary>
        /// <param name="target">The <see cref="ITarget"/> from which the passed <paramref name="lambda"/> was built</param>
        /// <param name="lambda">The lambda expression representing the code to be executed in order to get the underlying
        /// object which will be resolved.  Typically, this is fed directly from the
        /// <see cref="BuildResolveLambda(Expression, IExpressionCompileContext)"/> implementation.</param>
        protected virtual Func<ResolveContext, object> BuildFactory(ITarget target, Expression<Func<ResolveContext, object>> lambda)
        {
            return lambda.Compile();
        }

        protected virtual Func<ResolveContext, TService> BuildFactory<TService>(ITarget target, LambdaExpression lambda)
        {
            return (Func<ResolveContext, TService>)lambda.Compile();
        }

        private static Expression BuildFactoryBody(Expression expression, IExpressionCompileContext context)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // strip unnecessary conversions
            expression = new RedundantConvertRewriter().Visit(expression);

            // if we have shared conditionals, then we want to try and reorder them; as the intention
            // of the use of shared expressions is to consolidate them into one.  We do this on the boolean
            // expressions that might be used as tests for conditionals
            // Note that this is a tricky optimisation to understand - but the remarks section
            // of the ConditionalRewriter XML documentation contains a code-based example which should explain
            // what's going on.
            var sharedConditionalTests = context.SharedExpressions.Where(e => e.Type == typeof(bool)).ToArray();
            if (sharedConditionalTests.Length != 0)
            {
                expression = new ConditionalRewriter(expression, sharedConditionalTests).Rewrite();
            }

            // shared locals are local variables generated by targets that would normally be duplicated
            // if multiple targets of the same type are used in one compiled target.  By sharing them,
            // they reduce the size of the stack required for any generated code, but in turn
            // the compiler is required to lift them out and add them to an all-encompassing BlockExpression
            // surrounding all the code - otherwise they won't be in scope.
            var sharedLocals = context.SharedExpressions.OfType<ParameterExpression>().ToArray();
            if (sharedLocals.Length != 0)
            {
                expression = Expression.Block(expression.Type, sharedLocals, new BlockExpressionLocalsRewriter(sharedLocals).Visit(expression));
            }

            return expression;
        }

        /// <summary>
        /// Takes the unoptimised expression built for a target and optimises it and turns it into a lambda expression ready to
        /// be compiled into a factory delegate.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        public virtual Expression<Func<ResolveContext, object>> BuildResolveLambda(Expression expression, IExpressionCompileContext context)
        {
            expression = BuildFactoryBody(expression, context);

            // value types must be boxed, and that requires an explicit convert expression
            if (expression.Type != typeof(object) && expression.Type.IsValueType)
            {
                expression = Expression.Convert(expression, typeof(object));
            }

            return Expression.Lambda<Func<ResolveContext, object>>(expression, context.ResolveContextParameterExpression);
        }

        /// <summary>
        /// Equivalent to <see cref="BuildResolveLambda(Expression, IExpressionCompileContext)"/> except this produces a lambda whose delegate
        /// type is strongly typed for the type of object that's returned (instead of just being 'object').
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public LambdaExpression BuildResolveLambdaStrong(Expression expression, IExpressionCompileContext context)
        {
            return Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(ResolveContext), expression.Type),
                expression, 
                context.ResolveContextParameterExpression);
        }

        /// <summary>
        /// Called to build an expression for the specified target for the given <see cref="IExpressionCompileContext" /> - implementation
        /// of the <see cref="IExpressionCompiler.Build(ITarget, IExpressionCompileContext)"/> method.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built</param>
        /// <param name="context">The compilation context.</param>
        /// <exception cref="ArgumentException">If the compiler is unable to resolve an <see cref="IExpressionBuilder" /> from
        /// the <paramref name="context" /> for the <paramref name="target" /></exception>
        /// <remarks>This implementation first looks for an <see cref="IExpressionCompilationFilter"/> as an option from the 
        /// <paramref name="context"/>.  If one is obtained, its <see cref="IExpressionCompilationFilter.Intercept(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// method is called and, if it returns a non-null <see cref="Expression"/>, then that expression is used.
        /// 
        /// Otherwise, the normal behaviour is to attempt to resolve an <see cref="IExpressionBuilder{TTarget}" /> (with <c>TTarget"</c>
        /// equal to the runtime type of the <paramref name="target" />) or <see cref="IExpressionBuilder" /> whose
        /// <see cref="IExpressionBuilder.CanBuild(Type)" /> function returns <c>true</c> for the given target and context.
        /// If that lookup fails, then an <see cref="ArgumentException" /> is raised.  If the lookup succeeds, then the builder's
        /// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> function is called, and the expression it
        /// produces is returned.</remarks>
        public Expression Build(ITarget target, IExpressionCompileContext context)
        {
            if(target == null) throw new ArgumentNullException(nameof(target));
            if(context == null) throw new ArgumentNullException(nameof(context));

            var builder = ResolveBuilder(target, context)
                ?? throw new ArgumentException($"Unable to find an IExpressionBuilder for the target {target}", nameof(target));

            return builder.Build(target, context);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetCompiler.CreateContext(ResolveContext, ITargetContainer)"/>
        /// </summary>
        /// <param name="resolveContext"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public ICompileContext CreateContext(ResolveContext resolveContext, ITargetContainer targets)
        {
            return new ExpressionCompileContext(resolveContext, targets, resolveContext.RequestedType);
        }
    }
}
