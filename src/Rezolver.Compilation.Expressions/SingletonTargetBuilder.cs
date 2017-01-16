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
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.SingletonTarget}" />
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


		protected override Expression Build(SingletonTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//this isn't quite right, imho.  One singleton target should technically be able to create
			//more than one instance of a given concrete type, if that target is in a RezolverBuilder that is, 
			//in turn, used to build more than one top-level container (a permitted use case).
			//The way this code is written, however, the singleton will only ever create one instance of a type and,
			//in a cruel twist, will only ever belong to one scope (if applicable).
			//to fix this will mean being able to easily identify different container trees from each other,
			//and creating one instance for each unique tree.  This isn't even possible at the moment, because
			//there's no way to identify the root container in a chain of containers, unless you force the use
			//of scopes, which we don't necessarily want to do.
			//So I'm not sure if I've coded myself down a hole here - or if adding the necessary properties to 
			//the IRezolver interface to uniquely identify each one, and walk any parent/child relationship from 
			//child back to root would actually be a good thing to do in general anyway, so should therefore just
			//be done.

			var initialiser = target.GetOrAddInitialiser(context.TargetType ?? target.DeclaredType, t => {
				//get the underlying expression for the target that is to be turned into a singleton - but disable the
				//generation of any scope-tracking code.
				var innerExpression = compiler.BuildResolveLambda(target.InnerTarget, context.NewContext(t, suppressScopeTracking: true));
				
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
