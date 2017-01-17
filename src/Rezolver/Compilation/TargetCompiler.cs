// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rezolver.Compilation;

namespace Rezolver.Compilation
{
	/// <summary>
	/// Holds a reference to the default compiler for this application.
	/// </summary>
	public static class TargetCompiler
	{
		private class NullCompiler : ITargetCompiler
		{
			internal static NullCompiler Instance { get; } = new NullCompiler();

			private NullCompiler() { }

			public ICompiledTarget CompileTarget(ITarget target, ICompileContext context)
			{
				throw new InvalidOperationException("No default compiler has been set into Rezolver.Compilation.TargetCompiler.Default");
			}
		}

		private static ITargetCompiler _default = NullCompiler.Instance;
		/// <summary>
		/// The default compiler to be used by rezolvers when they are not explicitly provided one.
		/// </summary>
		public static ITargetCompiler Default
		{
			get { return _default; }
			set
			{
				value.MustNotBeNull("value");
				_default = value;
			}
		}
	}
}