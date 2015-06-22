using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
    public abstract class RezolveTargetCompilerBase : IRezolveTargetCompiler
    {
        public virtual ICompiledRezolveTarget CompileTarget(IRezolveTarget target, CompileContext context)
        {
            return CompileTargetBase(target, GetLambdaBody(target, context), context);
        }

        /// <summary>
        /// Produces the lambda body for the target.  The base class uses the method <see cref="ExpressionHelper.GetLambdaBodyForTarget(IRezolveTarget, CompileContext)"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Expression GetLambdaBody(IRezolveTarget target, CompileContext context)
        {
            return ExpressionHelper.GetLambdaBodyForTarget(target, context);
        }

        /// <summary>
        /// Called to create an ICompiledTarget instance from the passed expression produced by the passed target for the passed context.
        /// 
        /// The expression passed into this method is constructed by a call to <see cref="GetLambdaBody(IRezolveTarget, CompileContext)"/>
        /// </summary>
        /// <param name="target">The target from which the expression <paramref name="toCompile"/> was built.  Note - this expression will
        /// have been optimised and potentially rewritten ready for compilation, and will likely not be equal to the expression originally 
        /// produced by its own <see cref="IRezolveTarget.CreateExpression(CompileContext)"/> method.</param>
        /// <param name="toCompile">The expression built from <paramref name="target"/> by this instance's own <see cref="GetLambdaBody(IRezolveTarget, CompileContext)"/>
        /// methodd.</param>
        /// <param name="context">The context for which the compilation is being performed.</param>
        /// <returns></returns>
        protected abstract ICompiledRezolveTarget CompileTargetBase(IRezolveTarget target, Expression toCompile, CompileContext context);
    }
}
