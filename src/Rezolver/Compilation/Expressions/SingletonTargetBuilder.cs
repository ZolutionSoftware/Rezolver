// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised for the building the expression for a <see cref="SingletonTarget"/> target.
	/// </summary>
	public class SingletonTargetBuilder : ExpressionBuilderBase<SingletonTarget>
	{
		/// <summary>
		/// Used in ensuring the correct construction and scope tracking of a singleton instance.
		/// 
		/// Implements the ICompiledRezolveTarget interface for the single case of creating a lazy on demand (from the
		/// first ResolveContext that's passed to its GetObject method, and returning its value.
		/// </summary>
		private class SingletonTargetLazyInitialiser : ICompiledTarget
		{
			private Func<ResolveContext, Lazy<object>> _lazyFactory;
			private Lazy<object> _lazy;

			//internal Lazy<object> Lazy {  get { return _lazy; } }

			internal SingletonTargetLazyInitialiser(Func<ResolveContext, Lazy<object>> lazyFactory)
			{
				_lazyFactory = lazyFactory;
			}

			/// <summary>
			/// Creates the singleton target's Lazy on demand if required, and then returns the object that the underlying
			/// compiled code produces.
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			object ICompiledTarget.GetObject(ResolveContext context)
			{
				if (_lazy == null)
				{
					//here - if we create more than one lazy, it's not a big deal.  *Executing* two
					//different ones is a big deal.
					Interlocked.CompareExchange(ref _lazy, _lazyFactory(context), null);
				}
				return _lazy.Value;
			}
		}

		private static readonly MethodInfo ICompiledRezolveTarget_GetObject = MethodCallExtractor.ExtractCalledMethod((ICompiledTarget t) => t.GetObject(null));
		private static readonly ConstructorInfo LazyObject_Ctor = MethodCallExtractor.ExtractConstructorCall(() => new Lazy<object>(() => (object)null));


		/// <summary>
		/// Builds an expression for the given <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		protected override Expression Build(SingletonTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//TODO: Re below: Add a type/interface specifically to hold singletons in a container and add it to 
			//the configuration provider's set up.  Then we can use that as the backing store for the singleton 
			//objects produced by all singletons within.  In fact - we can use a similar approach to what's 
			//been done for scopes.  This enables us to have one singleton create multiple instances per-container-tree
			//as opposed to per app-domain.

			var initialiser = target.GetOrAddInitialiser(context.TargetType ?? target.DeclaredType, t => {
				//get the underlying expression for the target that is to be turned into a singleton - but disable any
				//scope tracking because the singleton is fixed to 'Implicit' and it's important that it is in
				//control of that, because all singletons must be stored in the very root scope.
				var innerExpression = compiler.BuildResolveLambda(target.InnerTarget, context.NewContext(t, scopeBehaviourOverride: ScopeBehaviour.None));
				
				//there's an argument for using the compiler here to generate this 
				//lambda, but our interface specifically creates an ICompiledRezolveTarget - not a delegate - so it's not 
				//really suitable.
				//in any case, compiling directly to a delegate will work on all platforms on which Rezolver will work in the 
				//first place, it just might not be as fast as it might be if we honoured the context's compiler, if
				//that happens to be the AssemblyRezolveTargetCompiler.
				var lazyLambdaExpr = Expression.Lambda(Expression.New(LazyObject_Ctor,
					//creating the factory delegate for the lazy uniquely for each invokation of the outer lambda
					//note that the RezolveContextExpression has to be quoted on the outer lambda, but will be used
					//on the inner lambda
					Expression.Lambda(innerExpression.Body)), context.ResolveContextExpression);
				var lazyLambda = (Func<ResolveContext, Lazy<object>>)lazyLambdaExpr.Compile();
				//now we create and capture an instance of the SingletonTargetLazyInitialiser class, passing our
				//dynamically constructed delegate along with this target
				return new SingletonTargetLazyInitialiser(lazyLambda);
			});

			return Expression.Call(Expression.Constant(initialiser, typeof(ICompiledTarget)),
				   ICompiledRezolveTarget_GetObject, context.ResolveContextExpression);
		}
	}
}
