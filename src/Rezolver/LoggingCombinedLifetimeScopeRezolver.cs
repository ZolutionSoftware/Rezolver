using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class LoggingCombinedLifetimeScopeRezolver : CombinedLifetimeScopeRezolver
	{
		private readonly int _id = LoggingDefaultRezolver.GetNextLoggingRezolverID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		public IRezolverLogger Logger { get; private set; }
		public LoggingCombinedLifetimeScopeRezolver(IRezolverLogger logger, 
			IRezolver inner, 
			IRezolverBuilder builder = null,
			IRezolveTargetCompiler compiler = null)
			: this(logger, null, inner, builder, compiler)
		{

		}

		public LoggingCombinedLifetimeScopeRezolver(IRezolverLogger logger, 
			ILifetimeScopeRezolver parentScope, 
			IRezolver inner = null, 
			IRezolverBuilder builder = null, 
			IRezolveTargetCompiler compiler = null)
			: base(parentScope, inner, builder ?? new LoggingRezolverBuilder(logger), compiler)
		{
			Logger = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.CanResolve(context), context);
		}

		protected override ILifetimeScopeRezolver CreateLifetimeScopeInstance()
		{
			return Logger.LogCallWithResult(this, () => base.CreateLifetimeScopeInstance());
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//TODO: change this to a LoggingCombinedLifetimeScopeRezolver
			return Logger.LogCallWithResult(this, () => base.CreateLifetimeScope());
		}

		public override ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.FetchCompiled(context), new { context = context });
		}

		protected override object GetService(Type serviceType)
		{
			return Logger.LogCallWithResult(this, () => base.GetService(serviceType), new { serviceType = serviceType });
		}

		public override object Resolve(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.Resolve(context), new { context = context });
		}

		public override bool TryResolve(RezolveContext context, out object result)
		{
			object tempResult = null;
			var @return = Logger.LogCallWithResult(this, () => base.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}

		public override void AddToScope(object obj, RezolveContext context = null)
		{
			Logger.LogCall(this, () => base.AddToScope(obj, context), new { obj = obj, context = context });
		}

		public override IEnumerable<object> GetFromScope(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.GetFromScope(context), new { context = context });
		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
		}

		protected override ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
		}
	}
}
