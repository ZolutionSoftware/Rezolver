using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// A rezolver that logs all calls through the IRezolver interface to aid debugging.
	/// </summary>
	public class LoggingDefaultRezolver : DefaultRezolver
	{
		protected IRezolverLogger Logger { get; private set; }


		public LoggingDefaultRezolver(IRezolverLogger logger, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null, bool registerToBuilder = true) :
			base(builder: builder, compiler: compiler, registerToBuilder: registerToBuilder)
		{
			Logger = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.CanResolve(context), context);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return Logger.LogCallWithResult(this, () => new LoggingCombinedLifetimeScopeRezolver(Logger, this));
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
