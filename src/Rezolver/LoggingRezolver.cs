using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
	public interface IRezolverLogger
	{
		int CallStart(RezolveContext context = null, [CallerMemberName]string method = null);
		void CallResult<TResult>(int reqId, TResult result);
		void CallEnd(int reqId);
		void Exception(int reqId, Exception ex);
	}

	public class LoggingLifetimeScopeResolver : LoggingRezolver, ILifetimeScopeRezolver
	{
		public LoggingLifetimeScopeResolver(ILifetimeScopeRezolver innerRezolver, IRezolverLogger logger)
			: base(innerRezolver, logger)
		{
			
		}

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void AddToScope(object obj, RezolveContext context = null)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected virtual bool Dispose(bool disposing)
		{

		}

		public IEnumerable<object> GetFromScope(RezolveContext context)
		{
			throw new NotImplementedException();
		}
	}

	public class LoggingRezolver : IRezolver
	{
		private readonly IRezolver _innerRezolver;
		private readonly IRezolverLogger _logger;

		public LoggingRezolver(IRezolver innerRezolver, IRezolverLogger logger)
		{
			_innerRezolver = innerRezolver;
			_logger = logger;
		}

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

		protected TResult LogCallWithResult<TResult>(Func<IRezolver, TResult> call, RezolveContext context = null, [CallerMemberName]string methodName = null)
		{
			var reqId = _logger.CallStart(context, methodName);

			TResult result;

			try
			{
				result = call(_innerRezolver);
			}
			catch(Exception ex)
			{
				_logger.Exception(reqId, ex);
				throw;
			}

			_logger.CallResult(reqId, result);
			return result;
		}

		public bool CanResolve(RezolveContext context)
		{
			return LogCallWithResult(r => r.CanResolve(context), context);
		}

		public ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return LogCallWithResult(r => new )
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			throw new NotImplementedException();
		}

		public object GetService(Type serviceType)
		{
			throw new NotImplementedException();
		}

		public object Resolve(RezolveContext context)
		{
			throw new NotImplementedException();
		}

		public bool TryResolve(RezolveContext context, out object result)
		{
			throw new NotImplementedException();
		}
	}

	//public class LoggingRezolver : DefaultRezolver
	//{
	//	public class LogEventArgs : EventArgs
	//	{
	//		public LogEventArgs(string message)
	//		{
	//			Message = message;
	//		}
	//		public string Message { get; private set; }
	//	}
	//	public event EventHandler<LogEventArgs> EventLogged;

	//	public LoggingRezolver(IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
	//		: base(builder, compiler)
	//	{

	//	}

	//	private void Log(string format, params object[] otherFormatArgs)
	//	{
	//		format = string.Format(format, otherFormatArgs);
	//		OnEventLogged(format);
	//	}

	//	private void LogWithContext(string format, RezolveContext context, params object[] otherFormatArgs)
	//	{
	//		string contextString = string.Format("[ Type: {0}, Name: {1}, With Scope? {2} ]", context.RequestedType, context.Name ?? "[null]", context.Scope != null ? "Yes" : "No");
	//		format = string.Format(format, contextString);
	//		Log(format, otherFormatArgs);
	//	}

	//	private void OnEventLogged(string eventMsg)
	//	{
	//		var evt = EventLogged;
	//		if (evt != null)
	//		{
	//			try
	//			{
	//				evt(this, new LogEventArgs(eventMsg));
	//			}
	//			catch (Exception) { }
	//		}
	//	}

	//	public override bool CanResolve(RezolveContext context)
	//	{
	//		var result = base.CanResolve(context);
	//		this.LogWithContext("CanResolve returning {{0}} for context {0}", context, result);
	//		return result;
	//	}

	//	public override ILifetimeScopeRezolver CreateLifetimeScope()
	//	{
	//		OnEventLogged("CreateLifetimeScope called");
	//		return base.CreateLifetimeScope();
	//	}

	//	protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
	//	{
	//		var result = base.GetCompiledRezolveTarget(context);
	//		this.LogWithContext("GetCompiledRezolveTarget returning compiled target {{0}} for context {0}", context, result != null ? result.GetType().ToString() : "[null]");
	//		return result;
	//	}

	//	public override object Resolve(RezolveContext context)
	//	{
	//		try { 
	//		var result = base.Resolve(context);
	//		LogWithContext("Resolve returning {{0}} for context {0}", context, result != null ? string.Format("instance of {0}", result.GetType()) : "[null]");
	//		return result;
	//			}
	//		catch(Exception ex)
	//		{
	//			this.LogWithContext("Resolve failed for context {0}. Exception Type: {{0}}, Message: \"{{1}}\"",
	//				context, ex.GetType(), ex.Message);
	//			throw;
	//		}
	//	}

	//	public override bool TryResolve(RezolveContext context, out object result)
	//	{
	//		var bResult = base.TryResolve(context, out result);
	//		LogWithContext("TryResolve returning {{0}} for context {0}", context, bResult ? string.Format("true, with instance of {0}", result.GetType()) : "false");
	//		return bResult;
	//	}
	//}
}
