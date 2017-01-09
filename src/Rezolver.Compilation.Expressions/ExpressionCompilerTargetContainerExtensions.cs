using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    public static class ExpressionCompilerTargetContainerExtensions
    {
		public static ITargetContainer UseExpressionCompiler(this ITargetContainer targets, ExpressionCompiler compiler = null)
		{
			compiler = compiler ?? ExpressionCompiler.Default;
			//will be how containers pick up and use this compiler
			targets.RegisterObject<ITargetCompiler>(compiler);
			//if you're looking to re-enter the compilation process for a particular
			//target - then you should request our compiler via the type IExpressionCompiler 
			targets.RegisterObject<IExpressionCompiler>(compiler);
			targets.RegisterObject<IExpressionBuilder<ConstructorTarget>>(new ConstructorTargetBuilder());

			return targets;
		}
    }
}