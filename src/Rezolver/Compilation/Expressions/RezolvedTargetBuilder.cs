// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised for building the expression for the <see cref="ResolvedTarget"/> target.
	/// </summary>
	public class RezolvedTargetBuilder : ExpressionBuilderBase<ResolvedTarget>
	{
		

        private static object DynamicResolve(IResolveContext context, Type newType, Func<IResolveContext, object> fallback)
        {
            //assumes context is already prepared with the correct type set
            object toReturn;
            if (!context.Container.TryResolve(context, out toReturn))
                toReturn = fallback(context);
            return toReturn;
        }

        private static readonly MethodInfo DynamicResolveMethod =
            MethodCallExtractor.ExtractCalledMethod(() => DynamicResolve(null, null, null));

        private MethodCallExpression CallDynamicResolve(Expression resolveContext, Expression type, Expression fallback)
        {
            return Expression.Call(DynamicResolveMethod, resolveContext, type, fallback);
        }

		/// <summary>
		/// Builds an expression for the given <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		/// <exception cref="System.InvalidOperationException"></exception>
		protected override Expression Build(ResolvedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//get the expression for the object that would be resolved statically.  If there is none,
			//then we emit a call back into the container that's passed in the context.

			//we must then generate a conditional expression which checks whether the 
			//container passed in the context (that's the parameter from the compile context) can rezolve
			//an object (optionally by the name, which might also be rezolved) and, if it can, call
			//that.  Only do this, though, if that container is different to the one to which
			//the target belongs.

			//this needs to do a check to see if the inbound container is different to the one we've got here
			//if it is, then we will defer to a resolve call on that container.  Otherwise we will use the static
			//target, or throw an exception.

			//try to resolve the target from the context.  Note this could resolve the fallback target.
			var staticTarget = target.Bind(context);
            //TODO: This should be a shared expression
            var currentContainer = context.CurrentContainerExpression;
			var declaredTypeExpr = Expression.Constant(target.DeclaredType, typeof(Type));
            var newContext = CallResolveContext_New(context.ResolveContextParameterExpression, declaredTypeExpr);
            /* new version */
            Expression staticExpr;

            if(staticTarget != null)
            {
                staticExpr = compiler.Build(staticTarget, context.NewContext(target.DeclaredType));
                if (staticExpr == null)
					throw new InvalidOperationException(string.Format(ExceptionResources.TargetReturnedNullExpressionFormat, staticTarget.GetType(), context.TargetType));
            }
            else
            {
                //this should generate a missing dependency exception if executed
                //or, might actually yield a result if registrations have been added
                //after the expression is compiled.
                staticExpr = CallIContainer_Resolve(currentContainer, newContext);
            }

            if (staticExpr.Type != target.DeclaredType)
                staticExpr = Expression.Convert(staticExpr, target.DeclaredType);

            var checkContainer = context.GetOrAddSharedExpression(typeof(bool),
                "IsSameRezolver",
                () => Expression.ReferenceEqual(context.ContextContainerPropertyExpression, currentContainer), this.GetType());
            

            Expression useContextRezolverIfCanExpr = Expression.Condition(
                CallIContainer_CanResolve(context.ContextContainerPropertyExpression, newContext),
                Expression.Convert(CallIContainer_Resolve(context.ContextContainerPropertyExpression, newContext), target.DeclaredType),
                staticExpr
              );

            var finalExpr = Expression.Condition(checkContainer,
                staticExpr,
                useContextRezolverIfCanExpr);

            return finalExpr;

            //var finalexpr = Expression.Block(new[] { originalContext, toReturn },
            //    Expression.Assign(originalContext, context.ResolveContextParameterExpression),
            //    Expression.Assign(context.ResolveContextParameterExpression,
            //        CallResolveContext_New(context.ResolveContextParameterExpression, newRequestedType: declaredTypeExpr)),
            //    Expression.Assign(toReturn,
            //        Expression.Condition(checkContainer,
            //            Expression.Convert(staticExpr, target.DeclaredType),
            //            Expression.Convert(CallDynamicResolve(context.ResolveContextParameterExpression,
            //                declaredTypeExpr,
            //                Expression.Constant(staticExprLambda.Compile())), target.DeclaredType))),
            //    Expression.Assign(context.ResolveContextParameterExpression, originalContext),
            //    toReturn);
            //return finalexpr;
            /* old version */
            //var newContextLocal = context.GetOrAddSharedLocal(typeof(IResolveContext), "newContext");
            //var newContextExpr = Expression.Call(context.ResolveContextParameterExpression,
            //    NewContextMethod,
            //    declaredTypeExpr,
            //    Expression.Default(typeof(IContainer)),
            //    Expression.Default(typeof(IContainerScope)));
            //var setNewContextLocal = Expression.Assign(context.ResolveContextParameterExpression, newContextExpr);
            //bool setNewContextFirst = false;
            //Expression staticExpr = null;

            //if (staticTarget != null)
            //{
            //	staticExpr = compiler.Build(staticTarget, context.NewContext(target.DeclaredType)); //need a new context here to change the resolve type to our declared type.
            //	if (staticExpr == null)
            //		throw new InvalidOperationException(string.Format(ExceptionResources.TargetReturnedNullExpressionFormat, staticTarget.GetType(), context.TargetType));
            //}
            //else
            //{
            //	//if no fallback call back into the container passed in the context to this method
            //	//when no target was found in the static search.  This is just a convenient way
            //	//to generate an exception saying that the dependency couldn't be found.
            //	//unless, of course, some naughty person has snuck in an additional registration
            //	//into the container after compilation has been done ;)
            //	setNewContextFirst = true;
            //	staticExpr = Expression.Call(currentContainer, RezolverResolveMethod, newContextLocal);
            //}

            //if (staticExpr.Type != target.DeclaredType)
            //	staticExpr = Expression.Convert(staticExpr, target.DeclaredType);

            ////TODO: Change this to use the TryResolve and assign to staticExpr if false - avoids the CanResolve/Resolve check
            ////Equivalent to (ResolveContext r) => r.Resolver.CanResolve(type) ? r.Resolver.Resolve<DeclaredType>() : <<staticExpr>>
            //Expression useContextRezolverIfCanExpr = Expression.Condition(Expression.Call(context.ContextContainerPropertyExpression, RezolverCanResolveMethod, newContextLocal),
            //	Expression.Convert(Expression.Call(context.ContextContainerPropertyExpression, RezolverResolveMethod, newContextLocal), target.DeclaredType),
            //	staticExpr
            //  );

            //List<Expression> blockExpressions = new List<Expression>();
            //if (setNewContextFirst)
            //	blockExpressions.Add(setNewContextLocal);
            //else
            //	useContextRezolverIfCanExpr = Expression.Block(target.DeclaredType, setNewContextLocal, useContextRezolverIfCanExpr);

            ////note the use of the shared expression here - which enables an advanced optimisation specifically connected with
            ////conditionals
            //blockExpressions.Add(Expression.Condition(context.GetOrAddSharedExpression(typeof(bool),
            //	"IsSameRezolver",
            //	() => Expression.ReferenceEqual(context.ContextContainerPropertyExpression, currentContainer), this.GetType()),
            //  staticExpr,
            //  useContextRezolverIfCanExpr));

            //if (blockExpressions.Count == 1)
            //	return blockExpressions[0];
            //else
            //	return Expression.Block(target.DeclaredType, blockExpressions);
        }
    }
}
