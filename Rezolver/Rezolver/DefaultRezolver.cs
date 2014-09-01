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
		public DefaultRezolver(IRezolverBuilder builder, IRezolveTargetCompiler compiler = null, bool enableDynamicRezolvers = DefaultEnableDynamicRezolvers)
			: base(enableDynamicRezolvers)
		{
			_builder = builder;
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