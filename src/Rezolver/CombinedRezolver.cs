using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Combines a new rezolver with an existing one.  Note that any compiled targets produced by the
    /// existing rezolver are retained, but can be overriden by any similar targets registered directly in 
    /// this one - including those that are used by other compiled targets from the inner one.  
	/// </summary>
	public class CombinedRezolver : DefaultRezolver
	{
		private readonly IRezolver _inner;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inner">The inner rezolver that this one combines with.  Any dependencies not served
		/// by the new combined rezolver's own registry will be sought from this rezolver.</param>
		public CombinedRezolver(IRezolver inner, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null) 
			: base(builder, compiler ?? inner.Compiler)
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
	}
}
