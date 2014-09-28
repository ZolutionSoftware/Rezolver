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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="compiler"></param>
		/// <param name="registerToBuilder"></param>
		public DefaultRezolver(IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null, bool registerToBuilder = true)
		{
			_builder = builder ?? new RezolverBuilder();
			_compiler = compiler;
			//auto-register this instance to the underlying builder.  This is so that framework-style components that
			//need to use dependency resolving instead of so-called 'pure' IOC can do so
			if(registerToBuilder)
			{
				_builder.Register(this.AsObjectTarget(), typeof(IRezolver));
			}
		}

		private IRezolveTargetCompiler _compiler;
		public override IRezolveTargetCompiler Compiler
		{
			get { return _compiler ?? RezolveTargetCompiler.Default; }
		}

		private IRezolverBuilder _builder;
		public override IRezolverBuilder Builder
		{
			get { return _builder; }
		}

	}
}