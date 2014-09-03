using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// </summary>
	public class DefaultRezolver : CachingRezolver
	{
		public DefaultRezolver(IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
		{
			_builder = builder ?? new RezolverBuilder();
			_compiler = compiler;
		}

		private IRezolveTargetCompiler _compiler;
		public override IRezolveTargetCompiler Compiler
		{
			get { return _compiler ?? RezolveTargetCompiler.Default; }
		}

		private IRezolverBuilder _builder;
		protected override IRezolverBuilder Builder
		{
			get { return _builder; }
		}

	}
}