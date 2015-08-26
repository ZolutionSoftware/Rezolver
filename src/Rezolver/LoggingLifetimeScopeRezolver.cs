using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
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

		protected TResult LogCallWithResult<TResult>(Func<ILifetimeScopeRezolver, TResult> call, dynamic arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(this, arguments, methodName);

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

		protected void LogCall(Action<ILifetimeScopeRezolver> call, dynamic arguments = null, [CallerMemberName]string methodName = null)
		{
			var reqId = Logger.CallStart(this, arguments, methodName);

			try
			{
				call(_innerScopeRezolver);
			}
			catch (Exception ex)
			{
				Logger.Exception(reqId, ex);
				throw;
			}

			Logger.CallEnd(reqId);
		}

		public void AddToScope(object obj, RezolveContext context = null)
		{
			LogCall(r => r.AddToScope(obj, context), new { obj = obj, context = context });
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
			return LogCallWithResult(r => r.GetFromScope(context), new { context = context });
		}
	}
}
