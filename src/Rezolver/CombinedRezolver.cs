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
		/// <param name="inner">Required.  The inner rezolver that this one combines with.  Any dependencies not served
		/// by the new combined rezolver's own registry will be sought from this rezolver.</param>
        /// <param name="builder">Optional. A specific builder to be used for this rezolver's own registrations.</param>
        /// <param name="compiler">Optional. A compiler to be used to create <see cref="ICompiledRezolveTarget"/> instances
        /// from this rezolver's registrations.  If this is not provided, then the <see cref="IRezolver.Compiler"/> of the
        /// <paramref name="inner"/> rezolver will be used.</param>
		public CombinedRezolver(IRezolver inner, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null) 
			: base(builder, compiler ?? inner.Compiler)
		{
			inner.MustNotBeNull("inner");
			_inner = inner;
		}

        /// <summary>
        /// Called to determine if this rezolver is able to resolve the type specified in the passed <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Required.  The <see cref="RezolveContext"/>.</param>
        /// <returns></returns>
		public override bool CanResolve(RezolveContext context)
		{
			return base.CanResolve(context) || _inner.CanResolve(context);
		}

        /// <summary>
        /// Overrides the base implementation to pass the lookup for an <see cref="IRezolveTarget"/> to the inner rezolver - this
        /// is how dependency chaining from this rezolver to the inner rezolver is achieved.
        /// </summary>
        /// <param name="context">Required.  The <see cref="RezolveContext"/>.</param>
        /// <returns></returns>
		protected override ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return _inner.FetchCompiled(context);
		}
	}
}
