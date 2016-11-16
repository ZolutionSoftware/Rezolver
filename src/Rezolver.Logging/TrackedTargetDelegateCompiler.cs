// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Decorates an existing ITargetCompiler instance to enable tracking of the calls made to it.
	/// This class is used by the tracked container classes in the library 
	/// </summary>
	/// <seealso cref="Rezolver.ITargetCompiler" />
	public class TrackedTargetDelegateCompiler : TargetDelegateCompiler
	{
		public TrackedTargetDelegateCompiler() { }

		protected override ICompiledTarget CompileTargetBase(ITarget target, Expression toCompile, CompileContext context)
		{
			return new DelegatingCompiledRezolveTarget(
				Expression.Lambda<Func<RezolveContext, object>>(toCompile, context.RezolveContextExpression).Compile());
		}
	}
}
