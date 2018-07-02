using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Delegate-based implementation of <see cref="IExpressionCompilationFilter"/> allowing multiple
    /// filters to be combined into one.
    /// </summary>
    public class ExpressionCompilationFilters : IExpressionCompilationFilter
    {
        private class DelegatedFilter : IExpressionCompilationFilter
        {
            private readonly Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression> _filter;

            public DelegatedFilter(Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression> filter)
            {
                _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            }

            public Expression Intercept(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler) => _filter(target, context, compiler);
        }

        private readonly List<IExpressionCompilationFilter> _filters;

        public ExpressionCompilationFilters(params Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression>[] delegatedFilters)
        {
            _filters = delegatedFilters.Select(f => (IExpressionCompilationFilter)new DelegatedFilter(f)).ToList();
        }

        public void Add(Func<ITarget, IExpressionCompileContext, IExpressionCompiler, Expression> filter) => _filters.Add(new DelegatedFilter(filter));

        public void Add(IExpressionCompilationFilter filter)
        {
            if (ReferenceEquals(this, filter))
                throw new ArgumentException("cannot add a filter to itself");
        }

        private bool _isIntercepting = false;
        public Expression Intercept(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            try
            {
                // first callback to return a non-null result wins
                if (_isIntercepting)
                    return null;
                _isIntercepting = true;

                return _filters.Select(f => f.Intercept(target, context, compiler)).FirstOrDefault();
            }
            finally
            {
                _isIntercepting = false;
            }

        }
    }
}
