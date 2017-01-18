// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Compilation;

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
			//and then we have all the expression builders.
			targets.RegisterObject<IExpressionBuilder<ConstructorTarget>>(new ConstructorTargetBuilder());

			return targets;
		}
    }
}