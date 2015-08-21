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
		protected TResult LogCallWithResult<TResult>(Func<IRezolver, TResult> call, RezolveContext context = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(context, methodName);

			TResult result;

			try
			{
				result = call(_innerRezolver);
			}
			catch(Exception ex)
			{
				Logger.Exception(reqId, ex);
				throw;
			}

			Logger.CallResult(reqId, result);
			return result;
		}

		protected void LogCall(Action<IRezolver> call, RezolveContext context = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(context, methodName);

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
			return LogCallWithResult(r => r.FetchCompiled(context));
		}

		public object GetService(Type serviceType)
		{
			return LogCallWithResult(r => r.GetService(serviceType));
		}

		public object Resolve(RezolveContext context)
		{
			return LogCallWithResult(r => r.Resolve(context));
		}

		public bool TryResolve(RezolveContext context, out object result)
		{
			object tempResult = null;
			var @return = LogCallWithResult(r => r.TryResolve(context, out tempResult));
			result = tempResult;
			return @return;
		}
	}

	public class LoggingLifetimeScopeResolver : LoggingRezolver, ILifetimeScopeRezolver
	{
		private bool _disposed;

		/// <summary>
		/// note - you'd expect this to be the same as the inner rezolver's parent, but it won't be, because of the way
		/// that this class decorates the inner rezolver.  In order for this to surface the correct parent, it has to be passed through,
		/// and it is expected to be a Logging wrapper of the parent of the inner rezolver.
		/// 
		/// Note also, not always set...
		/// </summary>
		private readonly ILifetimeScopeRezolver _parentScope;
		private readonly ILifetimeScopeRezolver _innerScopeRezolver;

		public LoggingLifetimeScopeResolver(ILifetimeScopeRezolver innerRezolver, IRezolverLogger logger, ILifetimeScopeRezolver parentScope = null)
			: base(innerRezolver, logger)
		{
			_parentScope = parentScope;
			_innerScopeRezolver = innerRezolver;
		}

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				return _parentScope;
			}
		}

		protected TResult LogCallWithResult<TResult>(Func<ILifetimeScopeRezolver, TResult> call, RezolveContext context = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(context, methodName);

			TResult result;

			try
			{
				result = call(_innerScopeRezolver);
			}
			catch (Exception ex)
			{
				Logger.Exception(reqId, ex);
				throw;
			}

			Logger.CallResult(reqId, result);
			return result;
		}

		protected void LogCall(Action<ILifetimeScopeRezolver> call, RezolveContext context = null, [CallerMemberName]string methodName = null)
		{

		}

		public void AddToScope(object obj, RezolveContext context = null)
		{
			//LogCall()
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_innerScopeRezolver.Dispose();
			}

			_disposed = true;
		}

		public IEnumerable<object> GetFromScope(RezolveContext context)
		{
			return LogCallWithResult(r => r.GetFromScope(context), context);
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
