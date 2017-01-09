﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{

	/// <summary>
	/// Implementation of the <see cref="ITargetCompiler"/> interface which produces <see cref="ICompiledTarget"/> objects
	/// by building and compiling expression trees from the <see cref="ITarget"/> objects which are registered.
	/// 
	/// To enable the use of this compiler, use the <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)"/>
	/// extension method on your root target container.
	/// </summary>
	/// <remarks>This class works by resolving <see cref="IExpressionBuilder"/> instances from the <see cref="CompileContext"/> which can 
	/// build an expression for a given <see cref="ITarget"/>.
	/// 
	/// Typically, this is done by searching for an <see cref="IExpressionBuilder{TTarget}"/> where 'TTarget' is equal to the runtime type
	/// of the target - e.g. <see cref="ConstructorTarget"/>.  If one cannot be found, it will then search for an <see cref="IExpressionBuilder"/>
	/// whose <see cref="IExpressionBuilder.CanBuild(ITarget, CompileContext)"/> function returns <c>true</c>.
	/// 
	/// With a correctly configured target dictionary (using the 
	/// <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)"/> extension method, for example), 
	/// this should resolve to an instance of the <see cref="ConstructorTargetBuilder"/> class, which implements 
	/// <code>IExpressionBuilder&lt;ConstructorTarget&gt;</code>
	/// 
	/// 
	/// As such, the compiler can be extended to support extra target types and its existing expression builders can be replaced for customised
	/// behaviour because they are all resolved from the <see cref="ITargetContainer"/> underpinning a particular <see cref="CompileContext"/>.
	/// 
	/// There is a caveat for this, however: you can't use the traditional targets (<see cref="ConstructorTarget"/> etc) to extend the compiler because
	/// they currently need to be compiled in order to work - and therefore cannot be used because it would cause an infinite recursion.  
	/// At present, therefore, the targets which are registered must directly implement either the <see cref="IExpressionBuilder{TTarget}"/> or 
	/// <see cref="IExpressionBuilder"/> interfaces; or implement the <see cref="ICompiledTarget"/> interface and produce an 
	/// instance of those interfaces when <see cref="ICompiledTarget.GetObject(RezolveContext)"/> is called on them.
	/// 
	/// Because of this requirement, the most common way to register an expression builder is to register an instance inside an 
	/// <see cref="ObjectTarget"/> against the correct type, because it also implements <see cref="ICompiledTarget"/> in addition 
	/// to <see cref="ITarget"/>.  
	/// 
	/// Using this pattern, it's therefore important that an expression builder is completely threadsafe and recursion safe (since one target's
	/// compilation might depend on the compilation of another of the same type).
	/// 
	/// 
	/// Under the default configuration, if you want to get hold of this compiler then you should request the type <see cref="IExpressionCompiler"/>,
	/// which is exclusively implemented by this type.
	/// </remarks>
	public class ExpressionCompiler : IExpressionCompiler, ITargetCompiler
	{
		/// <summary>
		/// Gets the default expression compiler which is, by default, registered into an <see cref="ITargetContainer"/> when
		/// the <see cref="ExpressionCompilerTargetContainerExtensions.UseExpressionCompiler(ITargetContainer)"/> method is called.
		/// </summary>
		public static ExpressionCompiler Default { get; } = new ExpressionCompiler();

		private class CompiledLambdaTarget : ICompiledTarget
		{
			private readonly Func<RezolveContext, object> _getObjectDelegate;

			public CompiledLambdaTarget(Func<RezolveContext, object> getObjectDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
			}

			public object GetObject(RezolveContext context)
			{
				return _getObjectDelegate(context);
			}
		}

		public ICompiledTarget CompileTarget(ITarget target, CompileContext context)
		{
			//if the target is already a compiledTarget, then return its result.
			if (target is ICompiledTarget)
				return (ICompiledTarget)target;

			var expression = Build(target, context);
			var lambda = CreateResolveLambda(expression, context);
			return CreateCompiledTargetForLambda(lambda);
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
		public virtual IExpressionBuilder ResolveBuilder(ITarget target, CompileContext context)
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
			foreach(var baseT in tTarget.GetAllBases().Where(t => TypeHelpers.IsAssignableFrom(typeof(ITarget), t)))
			{
				yield return baseT;
			}
		}

		protected virtual ICompiledTarget CreateCompiledTargetForLambda(Expression<Func<RezolveContext, object>> lambda)
		{
			return new CompiledLambdaTarget(lambda.Compile());
		}

		protected Expression<Func<RezolveContext, object>> CreateResolveLambda(Expression body, CompileContext context)
		{
			return Expression.Lambda<Func<RezolveContext, object>>(body, context.RezolveContextExpression);
		}

		/// <summary>
		/// Called to build an expression for the specified target for the given <see cref="CompileContext" /> - implementation 
		/// of the <see cref="IExpressionCompiler.Build(ITarget, CompileContext)"/> method.
		/// </summary>
		/// <param name="target">The target for which an expression is to be built</param>
		/// <param name="context">The compilation context.</param>
		/// <exception cref="ArgumentException">If the compiler is unable to resolve an <see cref="IExpressionBuilder" /> from
		/// the <paramref name="context" /> for the <paramref name="target" /></exception>
		/// <remarks>This implementation attempts to resolve an <see cref="IExpressionBuilder{TTarget}" /> (with <typeparamref name="TTarget" />
		/// equal to the runtime type of the <paramref name="target" />) or <see cref="IExpressionBuilder" /> whose
		/// <see cref="IExpressionBuilder.CanBuild(ITarget, CompileContext)" /> function returns <c>true</c> for the given target and context.
		/// If that lookup fails, then an <see cref="ArgumentException" /> is raised.  If the lookup succeeds, then the builder's
		/// <see cref="IExpressionBuilder.Build(ITarget, CompileContext)" /> function is called, and the expression it produces is returned.</remarks>
		public Expression Build(ITarget target, CompileContext context)
		{
			target.MustNotBeNull(nameof(target));
			context.MustNotBeNull(nameof(context));

			var builder = ResolveBuilder(target, context);
			if (builder == null)
				throw new ArgumentException($"Unable to find an IExpressionBuilder for the target { target }", nameof(target));
			return builder.Build(target, context);
		}
	}
}
