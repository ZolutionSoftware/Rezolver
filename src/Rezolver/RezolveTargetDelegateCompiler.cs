using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
    /// <summary>
    /// The default compiler for <see cref="IRezolveTarget"/> instances - takes the expression tree(s) produced by an
    /// <see cref="IRezolveTarget"/> and simply compiles it to an anonymous delegate.
    /// </summary>
	public class RezolveTargetDelegateCompiler : RezolveTargetCompilerBase
	{
		public static readonly IRezolveTargetCompiler Default = new RezolveTargetDelegateCompiler();

		public class DelegatingCompiledRezolveTarget : ICompiledRezolveTarget
		{
			private readonly Func<RezolveContext, object> _getObjectDelegate;

			public DelegatingCompiledRezolveTarget(Func<RezolveContext, object> getObjectDelegate)
			{
				_getObjectDelegate = getObjectDelegate;
			}

			public object GetObject(RezolveContext context)
			{
				return _getObjectDelegate(context);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="toCompile"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override ICompiledRezolveTarget CompileTargetBase(IRezolveTarget target, Expression toCompile, CompileContext context)
        {
#if DEBUG
            var expression =
                ExpressionHelper.GetResolveLambdaForExpression(toCompile, context);
            Debug.WriteLine("Compiling Func<RezolveContext, object> from static lambda {0} for target type {1}", expression, "System.Object");
            return new DelegatingCompiledRezolveTarget(expression.Compile());
#else
			return new DelegatingCompiledRezolveTarget(
				Expression.Lambda<Func<RezolveContext, object>>(toCompile, context.RezolveContextParameter).Compile());
#endif
        }
    }
}