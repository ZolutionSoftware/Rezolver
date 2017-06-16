// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{

    /// <summary>
    /// Implementation of the <see cref="ITargetCompiler" /> interface which produces <see cref="ICompiledTarget" /> objects
    /// by building and compiling expression trees from the <see cref="ITarget" /> objects which are registered.
    /// </summary>
    /// <seealso cref="Rezolver.Compilation.Expressions.IExpressionCompiler" />
    /// <seealso cref="Rezolver.Compilation.ITargetCompiler" />
    /// <remarks>Use of this compiler by a container is enabled by attaching the <see cref="Configuration.ExpressionCompilerBehaviour"/>
    /// <see cref="Configuration.ExpressionCompilerBehaviour.Instance"/> to the container either on construction (for example, via the 
    /// <see cref="Container.Container(ITargetContainer, IContainerConfig)"/> or
    /// <see cref="Container.Container(IContainerConfig)"/> constructors) or via the <see cref="GlobalBehaviours.ContainerBehaviour"/>
    /// global behaviour, which currently includes the behaviour by default.
    /// 
    /// This class works by directly resolving <see cref="IExpressionBuilder" /> instances which can build an expression for a 
    /// given <see cref="ITarget" /> from the <see cref="IExpressionCompileContext" />.
    /// 
    /// Typically, this is done by searching for an <see cref="IExpressionBuilder{TTarget}" /> where 'TTarget' is equal to the runtime type
    /// of the target - e.g. <see cref="Targets.ConstructorTarget" />.  If one cannot be found, it will then search for an <see cref="IExpressionBuilder" />
    /// whose <see cref="IExpressionBuilder.CanBuild(ITarget)" /> function returns <c>true</c> for the given target.
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
    /// Therefore, the targets which are registered as expression builders must directly implement either the 
    /// <see cref="IExpressionBuilder{TTarget}" /> or <see cref="IExpressionBuilder" /> interfaces; or implement the 
    /// <see cref="ICompiledTarget" /> interface and produce an instance of those interfaces when 
    /// <see cref="ICompiledTarget.GetObject(IResolveContext)" /> is called on them.
    /// 
    /// Because of this requirement, the most common way to register an expression builder is to register an instance inside an
    /// <see cref="Targets.ObjectTarget" /> against the correct type, because that class does implement <see cref="ICompiledTarget" /> 
    /// in addition to <see cref="ITarget" />.
    /// 
    /// Using this pattern, it's important that an expression builder is completely threadsafe and recursion safe (since one target's
    /// compilation might depend on the compilation of another of the same type).
    /// </remarks>
    public class ExpressionCompiler : IExpressionCompiler, ITargetCompiler
    {
        /// <summary>
        /// Gets the default expression compiler.  It's this that the <see cref="Configuration.ExpressionCompilerBehaviour"/>
        /// registers.
        /// </summary>
        public static ExpressionCompiler Default { get; } = new ExpressionCompiler();

        private class CompiledLambdaTarget : ICompiledTarget
        {
            private readonly Func<IResolveContext, object> _getObjectDelegate;

            public ITarget SourceTarget { get; }

            public CompiledLambdaTarget(ITarget sourceTarget, Func<IResolveContext, object> getObjectDelegate)
            {
                SourceTarget = sourceTarget;
                _getObjectDelegate = getObjectDelegate;
            }

            public object GetObject(IResolveContext context)
            {
                return _getObjectDelegate(context);
            }
        }

        /// <summary>
        /// Create the <see cref="ICompiledTarget" /> for the given <paramref name="target" /> using the <paramref name="context" />
        /// to inform the type of object that is to be built, and for compile-time dependency resolution.
        /// </summary>
        /// <param name="target">Required.  The target to be compiled.</param>
        /// <param name="context">Required.  The current compilation context.</param>
        /// <exception cref="System.ArgumentException">context must be an instance of IExpressionCompileContext</exception>
        public ICompiledTarget CompileTarget(ITarget target, ICompileContext context)
        {
            target.MustNotBeNull(nameof(target));
            context.MustNotBeNull(nameof(context));

            //if the target is already a compiledTarget, then cast it - its implementation
            //of ICompiledTarget is likely to be faster than any we could generate dynamically.
            if (target is ICompiledTarget compiledTarget)
                return compiledTarget;

            if (context is IExpressionCompileContext exprContext)
                return BuildCompiledTargetForLambda(target, this.BuildResolveLambda(target, exprContext));
            else
                throw new ArgumentException("context must be an instance of IExpressionCompileContext", nameof(context));
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
            target.MustNotBeNull(nameof(target));
            context.MustNotBeNull(nameof(context));

            var cache = context.ResolveContext.Resolve<ExpressionBuilderCache>();
            return cache.ResolveBuilder(target);
        }

        /// <summary>
        /// Creates an <see cref="ICompiledTarget"/> from the finalised <paramref name="lambda"/> expression which was
        /// previously built for a target.
        /// </summary>
        /// <param name="target">The <see cref="ITarget"/> from which the passed <paramref name="lambda"/> was built</param>
        /// <param name="lambda">The lambda expression representing the code to be executed in order to get the underlying
        /// object which will be resolved.  Typically, this is fed directly from the 
        /// <see cref="BuildResolveLambda(Expression, IExpressionCompileContext)"/> implementation.</param>
        protected virtual ICompiledTarget BuildCompiledTargetForLambda(ITarget target, Expression<Func<IResolveContext, object>> lambda)
        {
            return new CompiledLambdaTarget(target, lambda.Compile());
        }

        /// <summary>
        /// Takes the unoptimised expression built for a target and optimises it and turns it into a lambda expression ready to
        /// be compiled into an <see cref="ICompiledTarget"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The context.</param>
        public virtual Expression<Func<IResolveContext, object>> BuildResolveLambda(Expression expression, IExpressionCompileContext context)
        {
            expression.MustNotBeNull(nameof(expression));
            context.MustNotBeNull(nameof(context));

            //strip unnecessary conversions
            expression = new RedundantConvertRewriter().Visit(expression);

            //if we have shared conditionals, then we want to try and reorder them; as the intention
            //of the use of shared expressions is to consolidate them into one.  We do this on the boolean
            //expressions that might be used as tests for conditionals
            //Note that this is a tricky optimisation to understand - but the remarks section
            //of the ConditionalRewriter XML documentation contains a code-based example which should explain
            //what's going on.
            var sharedConditionalTests = context.SharedExpressions.Where(e => e.Type == typeof(Boolean)).ToArray();
            if (sharedConditionalTests.Length != 0)
                expression = new ConditionalRewriter(expression, sharedConditionalTests).Rewrite();

            //shared locals are local variables generated by targets that would normally be duplicated
            //if multiple targets of the same type are used in one compiled target.  By sharing them,
            //they reduce the size of the stack required for any generated code, but in turn 
            //the compiler is required to lift them out and add them to an all-encompassing BlockExpression
            //surrounding all the code - otherwise they won't be in scope.
            var sharedLocals = context.SharedExpressions.OfType<ParameterExpression>().ToArray();
            if (sharedLocals.Length != 0)
            {
                expression = Expression.Block(expression.Type, sharedLocals, new BlockExpressionLocalsRewriter(sharedLocals).Visit(expression));
            }

            //value types must be boxed, and that requires an explicit convert expression
            if (expression.Type != typeof(object) && TypeHelpers.IsValueType(expression.Type))
                expression = Expression.Convert(expression, typeof(object));

            return Expression.Lambda<Func<IResolveContext, object>>(expression, context.ResolveContextParameterExpression);
        }

        /// <summary>
        /// Called to build an expression for the specified target for the given <see cref="IExpressionCompileContext" /> - implementation 
        /// of the <see cref="IExpressionCompiler.Build(ITarget, IExpressionCompileContext)"/> method.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built</param>
        /// <param name="context">The compilation context.</param>
        /// <exception cref="ArgumentException">If the compiler is unable to resolve an <see cref="IExpressionBuilder" /> from
        /// the <paramref name="context" /> for the <paramref name="target" /></exception>
        /// <remarks>This implementation attempts to resolve an <see cref="IExpressionBuilder{TTarget}" /> (with <c>TTarget"</c>
        /// equal to the runtime type of the <paramref name="target" />) or <see cref="IExpressionBuilder" /> whose
        /// <see cref="IExpressionBuilder.CanBuild(ITarget)" /> function returns <c>true</c> for the given target and context.
        /// If that lookup fails, then an <see cref="ArgumentException" /> is raised.  If the lookup succeeds, then the builder's
        /// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)" /> function is called, and the expression it
        /// produces is returned.</remarks>
        public Expression Build(ITarget target, IExpressionCompileContext context)
        {
            target.MustNotBeNull(nameof(target));
            context.MustNotBeNull(nameof(context));

            var builder = ResolveBuilder(target, context) 
                ?? throw new ArgumentException($"Unable to find an IExpressionBuilder for the target { target }", nameof(target));
            return builder.Build(target, context);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetCompiler.CreateContext(IResolveContext, ITargetContainer)"/>
        /// </summary>
        /// <param name="resolveContext"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public ICompileContext CreateContext(IResolveContext resolveContext, ITargetContainer targets)
        {
            return new ExpressionCompileContext(resolveContext, targets, resolveContext.RequestedType);
        }
    }
}
