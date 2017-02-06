// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{

	/// <summary>
	/// Implementation of the <see cref="ITargetCompiler" /> interface which produces <see cref="ICompiledTarget" /> objects
	/// by building and compiling expression trees from the <see cref="ITarget" /> objects which are registered.
	/// To enable the use of this compiler, use the <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)" />
	/// extension method on your root target container.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.IExpressionCompiler" />
	/// <seealso cref="Rezolver.ITargetCompiler" />
	/// <remarks>To configure this compiler as the default for all containers (at least, those inheriting from <see cref="ContainerBase"/>,
	/// simply call the 
	/// 
	/// This class works by directly resolving <see cref="IExpressionBuilder" /> instances which can build an expression for a 
	/// given <see cref="ITarget" /> from the <see cref="IExpressionCompileContext" />.
	/// 
	/// Typically, this is done by searching for an <see cref="IExpressionBuilder{TTarget}" /> where 'TTarget' is equal to the runtime type
	/// of the target - e.g. <see cref="ConstructorTarget" />.  If one cannot be found, it will then search for an <see cref="IExpressionBuilder" />
	/// whose <see cref="IExpressionBuilder.CanBuild(ITarget, CompileContext)" /> function returns <c>true</c> for the given target.
	/// 
	/// With a correctly configured target dictionary (using the
	/// <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)" /> extension method, for example),
	/// this should resolve to an instance of the <see cref="ConstructorTargetBuilder" /> class, which implements
	/// <c>IExpressionBuilder&lt;ConstructorTarget&gt;</c>.
	/// 
	/// As such, the compiler can be extended to support extra target types and its existing expression builders can be replaced for customised
	/// behaviour because they are all resolved from the <see cref="ITargetContainer" /> underpinning a particular <see cref="CompileContext" />.
	/// There is a caveat for this, however: you can't use the traditional targets (<see cref="ConstructorTarget" /> etc) to extend the compiler because
	/// they currently need to be compiled in order to work - and therefore cannot be used because it would cause an infinite recursion.
	/// 
	/// At present, therefore, the targets which are registered as expression builders must directly implement either the 
	/// <see cref="IExpressionBuilder{TTarget}" /> or <see cref="IExpressionBuilder" /> interfaces; or implement the 
	/// <see cref="ICompiledTarget" /> interface and produce an instance of those interfaces when 
	/// <see cref="ICompiledTarget.GetObject(ResolveContext)" /> is called on them.
	/// 
	/// Because of this requirement, the most common way to register an expression builder is to register an instance inside an
	/// <see cref="ObjectTarget" /> against the correct type, because it also implements <see cref="ICompiledTarget" /> in addition
	/// to <see cref="ITarget" />.
	/// 
	/// Using this pattern, it's important that an expression builder is completely threadsafe and recursion safe (since one target's
	/// compilation might depend on the compilation of another of the same type).
	/// Under the default configuration, if you want to get hold of this compiler then you should request the type <see cref="IExpressionCompiler" />
	/// from the current compilation context, or from your target container.
	/// </remarks>
	public class ExpressionCompiler : IExpressionCompiler, ITargetCompiler, ICompileContextProvider
	{
		/// <summary>
		/// Gets the default expression compiler which is, by default, registered into an <see cref="ITargetContainer"/> when
		/// the <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)"/> method is called.
		/// </summary>
		public static ExpressionCompiler Default { get; } = new ExpressionCompiler();

		/// <summary>
		/// Gets the default provider.
		/// </summary>
		/// <value>The provider.</value>
		public static ExpressionCompilerConfigurationProvider ConfigProvider { get; } = new ExpressionCompilerConfigurationProvider();

		/// <summary>
		/// Configures the Rezolver <see cref="CompilerConfiguration" /> to use the default expression compiler for
		/// all containers not otherwise supplied with a specific compiler configuration
		/// </summary>
		public static void UseAsDefaultCompiler()
		{
			CompilerConfiguration.Default = ConfigProvider;
		}


		private class CompiledLambdaTarget : ICompiledTarget
		{
			private readonly Func<ResolveContext, object> _getObjectDelegate;

			public CompiledLambdaTarget(Func<ResolveContext, object> getObjectDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
			}

			public object GetObject(ResolveContext context)
			{
				return _getObjectDelegate(context);
			}
		}

		public ICompiledTarget CompileTarget(ITarget target, ICompileContext context)
		{
			target.MustNotBeNull(nameof(target));
			context.MustNotBeNull(nameof(context));
			
			//if the target is already a compiledTarget, then cast it - its implementation
			//of ICompiledTarget is likely to be faster than any we could generate dynamically.
			if (target is ICompiledTarget)
				return (ICompiledTarget)target;

			var exprContext = context as IExpressionCompileContext;
			if (exprContext == null)
				throw new ArgumentException("context must be an instance of IExpressionCompileContext", nameof(context));
			return BuildCompiledTargetForLambda(BuildResolveLambda(target, exprContext));
		}


		/// <summary>
		/// Similar to the <see cref="Build(ITarget, ICompileContext)" /> function, except the returned lambda will be optimised and can be
		/// immediately compiled into a delegate and executed; or quoted inside another expression as a callback.
		/// The <see cref="ICompileContext.RezolveContextExpression" /> will be passed to define the single parameter for the lambda
		/// that is created.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="context">The current compilation context.</param>
		public virtual Expression<Func<ResolveContext, object>> BuildResolveLambda(ITarget target, IExpressionCompileContext context)
		{
			var expression = Build(target, context);

			//strip unnecessary conversions
			expression = new RedundantConvertRewriter().Visit(expression);

			//if we have shared conditionals, then we want to try and reorder them; as the intention
			//of the use of shared expressions is to consolidate them into one.  We do this on the boolean
			//expressions that might be used as tests for conditionals
			//Note that this is an absolute nightmare of an optimisation to understand - but the remarks section
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

			return BuildResolveLambda(expression, context);
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

			List<Type> builderTypes =
				TargetSearchTypes(target).Select(t => typeof(IExpressionBuilder<>).MakeGenericType(t)).ToList();

			//and add the IExpressionBuilder type
			builderTypes.Add(typeof(IExpressionBuilder));

			foreach (var type in builderTypes)
			{
				foreach (IExpressionBuilder expressionBuilder in context.FetchAllDirect(type))
				{
					if (expressionBuilder != this)
					{
						if (expressionBuilder.CanBuild(target, context))
							return expressionBuilder;
					}
				}
			}

			return null;
		}

		private IEnumerable<Type> TargetSearchTypes(ITarget target)
		{
			//the search list is:
			//1) the target's type
			//2) each base (in descending order of inheritance) of the target which is compatible with ITarget
			//So a target of type {MyTarget<T> : TargetBase} will yield the list
			//[ MyTarget<T>, TargetBase ]
			//Whereas a target of type MyTarget<T> : ITarget yields the list
			//[ MyTarget<T> ]
			//because it has no bases which implement ITarget

			var tTarget = target.GetType();
			yield return tTarget;
			//return all bases which can be treated as ITarget
			foreach (var baseT in tTarget.GetAllBases().Where(t => TypeHelpers.IsAssignableFrom(typeof(ITarget), t)))
			{
				yield return baseT;
			}
		}

		protected virtual ICompiledTarget BuildCompiledTargetForLambda(Expression<Func<ResolveContext, object>> lambda)
		{
			return new CompiledLambdaTarget(lambda.Compile());
		}

		protected Expression<Func<ResolveContext, object>> BuildResolveLambda(Expression expression, IExpressionCompileContext context)
		{
			//value types must be boxed, and that requires an explicit convert expression
			if (expression.Type != typeof(object) && TypeHelpers.IsValueType(expression.Type))
				expression = Expression.Convert(expression, typeof(object));

			return Expression.Lambda<Func<ResolveContext, object>>(expression, context.ResolveContextExpression);
		}

		/// <summary>
		/// Called to build an expression for the specified target for the given <see cref="IExpressionCompileContext" /> - implementation 
		/// of the <see cref="IExpressionCompiler.Build(ITarget, IExpressionCompileContext)"/> method.
		/// </summary>
		/// <param name="target">The target for which an expression is to be built</param>
		/// <param name="context">The compilation context.</param>
		/// <exception cref="ArgumentException">If the compiler is unable to resolve an <see cref="IExpressionBuilder" /> from
		/// the <paramref name="context" /> for the <paramref name="target" /></exception>
		/// <remarks>This implementation attempts to resolve an <see cref="IExpressionBuilder{TTarget}" /> (with <typeparamref name="TTarget" />
		/// equal to the runtime type of the <paramref name="target" />) or <see cref="IExpressionBuilder" /> whose
		/// <see cref="IExpressionBuilder.CanBuild(ITarget, IExpressionCompileContext)" /> function returns <c>true</c> for the given target and context.
		/// If that lookup fails, then an <see cref="ArgumentException" /> is raised.  If the lookup succeeds, then the builder's
		/// <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext)" /> function is called, and the expression it produces is returned.</remarks>
		public Expression Build(ITarget target, IExpressionCompileContext context)
		{
			target.MustNotBeNull(nameof(target));
			context.MustNotBeNull(nameof(context));

			var builder = ResolveBuilder(target, context);
			if (builder == null)
				throw new ArgumentException($"Unable to find an IExpressionBuilder for the target { target }", nameof(target));
			return builder.Build(target, context);
		}

		public ICompileContext CreateContext(ResolveContext resolveContext, ITargetContainer targets, IContainer containerOverride = null)
		{
			return new ExpressionCompileContext(containerOverride ?? resolveContext.Container,
				targets, resolveContext.RequestedType);
		}
	}
}
