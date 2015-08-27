using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// 
	/// </summary>
	public class LoggingRezolver : IRezolver
	{
		private readonly IRezolver _innerRezolver;

		protected IRezolverLogger Logger { get; private set; }


		public IRezolverBuilder Builder
		{
			get
			{
				return _innerRezolver.Builder;
			}
		}

		public IRezolveTargetCompiler Compiler
		{
			get
			{
				return _innerRezolver.Compiler;
			}
		}

		public LoggingRezolver(IRezolver innerRezolver, IRezolverLogger logger)
		{
			_innerRezolver = innerRezolver;
			Logger = logger;
		}

		/// <summary>
		/// Log a call to a method with 
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="call"></param>
		/// <param name="context"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		protected TResult LogCallWithResult<TResult>(Func<IRezolver, TResult> call, dynamic arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(this, arguments, methodName);

			TResult result;

			try
			{
				result = call(_innerRezolver);
			}
			catch (Exception ex)
			{
				Logger.Exception(reqId, ex);
				throw;
			}

			Logger.CallResult(reqId, result);
			return result;
		}

		protected void LogCall(Action<IRezolver> call, dynamic arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(this, arguments, methodName);

			try
			{
				call(_innerRezolver);
			}
			catch (Exception ex)
			{
				Logger.Exception(reqId, ex);
				throw;
			}

			Logger.CallEnd(reqId);
		}

		public bool CanResolve(RezolveContext context)
		{
			return LogCallWithResult(r => r.CanResolve(context), context);
		}

		public ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return LogCallWithResult(r => new LoggingLifetimeScopeResolver(r.CreateLifetimeScope(), Logger));
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			return LogCallWithResult(r => r.FetchCompiled(context), new { context = context });
		}

		public object GetService(Type serviceType)
		{
			return LogCallWithResult(r => r.GetService(serviceType), new { serviceType = serviceType });
		}

		public object Resolve(RezolveContext context)
		{
			return LogCallWithResult(r => r.Resolve(context), new { context = context });
		}

		public bool TryResolve(RezolveContext context, out object result)
		{
			object tempResult = null;
			var @return = LogCallWithResult(r => r.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}
	}
}
