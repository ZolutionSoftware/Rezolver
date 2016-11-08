// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Decorates an existing ITargetCompiler instance to enable tracking of the calls made to it.
	/// </summary>
	/// <seealso cref="Rezolver.ITargetCompiler" />
	public class TrackedCompiler : ITargetCompiler
	{
		public TrackedCompiler(ITargetCompiler inner = null)
		{
			inner = inner ?? TargetCompiler.Default;
		}

		public ICompiledTarget CompileTarget(ITarget target, CompileContext context)
		{
			throw new NotImplementedException();
		}
	}
}
