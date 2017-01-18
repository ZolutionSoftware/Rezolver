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
	/// An <see cref="IExpressionBuilder"/> specialised for building expressions for <see cref="ScopedTarget"/> targets.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.ScopedTarget}" />
	public class ScopedTargetBuilder : ExpressionBuilderBase<ScopedTarget>
	{
		private ConstructorInfo _argExceptionCtor = 
			MethodCallExtractor.ExtractConstructorCall(() => new ArgumentException("", ""));

		protected override Expression Build(ScopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			throw new NotImplementedException();
			//TODO: Scoping must be implemented differently, to remove the need for any code generation

			//if the lifetime scope is null then throw an exception, because the lack of a scope 
			//prevents the scoped object from doing what it's supposed to do.
			var isScopeNull = Expression.Equal(context.ContextScopePropertyExpression, Expression.Default(typeof(IScopedContainer)));
			var throwArgException = Expression.Throw(Expression.New(_argExceptionCtor,
				Expression.Property(null, typeof(ExceptionResources), "ScopedSingletonRequiresAScope"),
				Expression.Constant(context.ResolveContextExpression.Name ?? "context")));

			var actualType = context.TargetType ?? target.DeclaredType;

			return Expression.Block(
				Expression.IfThen(isScopeNull, throwArgException),
				ExpressionHelper.Make_Scope_GetOrAddCallExpression(context,
					actualType,
					//the second parameter to the function is a callback that is to be executed when the object
					//is to be created - for that, instead of getting the raw expression for the target, we get
					//the final lambda that would normally be compiled for the target and bake it directly as
					//the callback.
					compiler.BuildResolveLambda(target.InnerTarget, context.NewContext(actualType, suppressScopeTracking: true)),
					Expression.Constant(false))
				);
		}
	}
}
