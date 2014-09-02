using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Combines a new rezolver with an existing one.
	/// </summary>
	public class CombinedRezolver : DefaultRezolver
	{
		private readonly IRezolver _inner;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second">Note, you can pass null - in which case this instance
		/// simply acts as a proxy for the first rezolver.</param>
		public CombinedRezolver(IRezolver inner, IRezolverBuilder builder, IRezolveTargetCompiler compiler) 
			: base(builder, compiler)
		{
			inner.MustNotBeNull("inner");
			_inner = inner;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return base.CanResolve(context) || _inner.CanResolve(context);
		}

		protected override ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return _inner.FetchCompiled(context);
		}
 
		public override object Resolve(RezolveContext context)
		{
			return base.Resolve(context);
		}
	}
}
