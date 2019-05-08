// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An implementation of <see cref="ExpressionBuilderBase{TTarget}"/> specialised for the target type <see cref="EnumerableTarget"/>.
    /// </summary>
    public class EnumerableTargetBuilder : ExpressionBuilderBase<EnumerableTarget>
    {
        /// <summary>
        /// Builds an expression which represents an instance of <see cref="IEnumerable{T}"/> whose elements are created by the
        /// <see cref="EnumerableTarget.Targets"/> of the passed <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built.</param>
        /// <param name="context">The current compilation context.</param>
        /// <param name="compiler">The compiler to use when building expressions for child targets.</param>
        /// <returns>An expression which can be compiled into a delegate that, when executed, will create an instance of the enumerable
        /// represented by <paramref name="target"/>
        /// </returns>
        /// <remarks>
        /// The compiler is capable of producing both lazy-loaded and eager-loaded enumerables, which can be controlled via
        /// target container options.
        ///
        /// ## Lazy vs Eager loading
        ///
        /// The option <see cref="Options.LazyEnumerables"/> is read from the <paramref name="context"/> for the
        /// <see cref="EnumerableTarget.ElementType"/> of the <paramref name="target"/>.  If it is equivalent to <c>true</c>
        /// (the <see cref="Options.LazyEnumerables.Default"/>), then a lazily-loaded enumerable is constructed which will
        /// create new instances of each object in the enumerable each time it is enumerated.
        ///
        /// If the option is instead equivalent to <c>false</c>, then all instances will be created in advance, and an already-materialised
        /// enumerable is constructed.</remarks>
        protected override Expression Build(EnumerableTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            if (context.GetOption(target.ElementType, Options.LazyEnumerables.Default))
            {
                var funcs =
                    target.Targets.Select(t => compiler.BuildResolveLambdaStrong(t, context.NewContext(target.ElementType)).Compile())
                    .ToArray();

                var lazyType = typeof(LazyEnumerable<>).MakeGenericType(target.ElementType);

                var ctor = lazyType.GetConstructor(new[] { typeof(Delegate[]) });

                var lazy = ctor.Invoke(new object[] { funcs });

                return Expression.Call(
                    Expression.Constant(lazy),
                    "GetInstances",
                    null,
                    context.ResolveContextParameterExpression);
            }
            else
            {
                List<Expression> all = new List<Expression>();

                for(var f = 0; f<target.Targets.Length; f++)
                {
                    all.Add(compiler.Build(target.Targets[f], context.NewContext(target.ElementType)));
                }

                return Expression.New(
                    typeof(EagerEnumerable<>).MakeGenericType(target.ElementType).GetConstructors()[0],
                    Expression.NewArrayInit(target.ElementType, all));
            }
        }
    }
}
