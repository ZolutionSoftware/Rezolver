using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
	public class LoggingLifetimeScopeResolver : DefaultLifetimeScopeRezolver
	{
		private readonly int _id = LoggingDefaultRezolver.GetNextLoggingRezolverID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		private bool _disposed;
	
		protected IRezolverLogger Logger { get; private set; }

		public LoggingLifetimeScopeResolver(IRezolverLogger logger, 
			IRezolverBuilder builder = null,
			IRezolveTargetCompiler compiler = null, 
			ILifetimeScopeRezolver parentScope = null, 
			bool registerToBuilder = true)
			: base(builder: builder ?? new LoggingRezolverBuilder(logger), compiler: compiler, registerToBuilder:registerToBuilder)
		{
			logger.MustNotBeNull(nameof(logger));
			Logger = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.CanResolve(context), context);
		}

		protected override ILifetimeScopeRezolver CreateLifetimeScopeInstance()
		{
			return new LoggingCombinedLifetimeScopeRezolver(Logger, this);
		}
		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
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

		public object Resolve(RezolveContext context)
		{
			return Logger.LogCallWithResult(this, () => base.Resolve(context), new { context = context });
		}

		public bool TryResolve(RezolveContext context, out object result)
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
	}
}
