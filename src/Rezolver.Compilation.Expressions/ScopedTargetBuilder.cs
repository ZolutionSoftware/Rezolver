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
	/// An <see cref="IExpressionBuilder"/> specialised for building expressions for <see cref="ScopedTarget"/> targets.
	/// </summary>
	public class ScopedTargetBuilder : ExpressionBuilderBase<ScopedTarget>
	{
		private ConstructorInfo _argExceptionCtor = 
			MethodCallExtractor.ExtractConstructorCall(() => new ArgumentException("", ""));

		protected override Expression Build(ScopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//all we need to do is force the inner target's scope behaviour to None - and this builder's
			//base code will ensure that the whole resulting expression is converted into an explicitly scoped one

			//note that this scope deactivation is only in place for this one target - if it has any child targets then
			//scoping behaviour for those returns to normal if compiled with a new context (which they always should be)

			return compiler.Build(target.InnerTarget, context.NewContext(scopeBehaviourOverride: ScopeActivationBehaviour.None));
			////TODO: Scoping must be implemented differently, to remove the need for any code generation

			////if the lifetime scope is null then throw an exception, because the lack of a scope 
			////prevents the scoped object from doing what it's supposed to do.
			//var isScopeNull = Expression.Equal(context.ContextScopePropertyExpression, Expression.Default(typeof(IScopedContainer)));
			//var throwArgException = Expression.Throw(Expression.New(_argExceptionCtor,
			//	Expression.Property(null, typeof(ExceptionResources), "ScopedSingletonRequiresAScope"),
			//	Expression.Constant(context.ResolveContextExpression.Name ?? "context")));

			//var actualType = context.TargetType ?? target.DeclaredType;

			//return Expression.Block(
			//	Expression.IfThen(isScopeNull, throwArgException),
			//	ExpressionHelper.Make_Scope_GetOrAddCallExpression(context,
			//		actualType,
			//		//the second parameter to the function is a callback that is to be executed when the object
			//		//is to be created - for that, instead of getting the raw expression for the target, we get
			//		//the final lambda that would normally be compiled for the target and bake it directly as
			//		//the callback.
			//		compiler.BuildResolveLambda(target.InnerTarget, context.NewContext(actualType, suppressScopeTracking: true)),
			//		Expression.Constant(false))
			//	);
		}
	}
}
