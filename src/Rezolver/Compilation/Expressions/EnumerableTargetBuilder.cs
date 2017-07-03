using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Rezolver.Targets;
using System.Linq;
using System.Reflection;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An implementation of <see cref="ExpressionBuilderBase{TTarget}"/> specialised for the target type <see cref="EnumerableTarget"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class EnumerableTargetBuilder : ExpressionBuilderBase<EnumerableTarget>
    {
        internal class LazyEnumerable<T> : IEnumerable<T>
        {
            private readonly IResolveContext _context;
            private readonly IEnumerable<Func<IResolveContext, object>> _factories;

            public LazyEnumerable(IResolveContext context, IEnumerable<Func<IResolveContext, object>> factories)
            {
                _context = context;
                _factories = factories;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _factories.Select(f => (T)f(_context)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

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
        /// The option <see cref="Options.LazyLoadedEnumerables"/> is read from the <paramref name="context"/> for the 
        /// <see cref="ITarget.DeclaredType"/> of the <paramref name="target"/>.  If it is equivalent to <c>true</c> 
        /// (the <see cref="Options.LazyLoadedEnumerables.Default"/>), then a lazily-loaded enumerable is constructed which will
        /// create new instances of each object in the enumerable each time it is enumerated.
        /// 
        /// If the option is instead equivalent to <c>false</c>, then all instances will be created in advance, and an already-materialised
        /// enumerable is constructed.</remarks>
        protected override Expression Build(EnumerableTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            if (context.GetOption(target.DeclaredType, Options.LazyLoadedEnumerables.Default))
            {
                var all = target.Targets.Select(t => compiler.BuildResolveLambda(t, context.NewContext(target.ElementType)));
                return Expression.New(
                    TypeHelpers.GetConstructors(typeof(LazyEnumerable<>).MakeGenericType(target.ElementType)).Single(),
                    context.ResolveContextParameterExpression,
                    Expression.NewArrayInit(
                        typeof(Func<IResolveContext, object>),
                        all));
            }
            else
            {
                return Expression.NewArrayInit(target.ElementType,
                    target.Targets.Select(t => compiler.Build(t, context.NewContext(target.ElementType))));
            }
        }
    }
}
