using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class LoggingRezolver : DefaultRezolver
	{
		public class LogEventArgs : EventArgs
		{
			public LogEventArgs(string message)
			{
				Message = message;
			}
			public string Message { get; private set; }
		}
		public event EventHandler<LogEventArgs> EventLogged;

		public LoggingRezolver(IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
			: base(builder, compiler)
		{

		}

		private void Log(string format, params object[] otherFormatArgs)
		{
			format = string.Format(format, otherFormatArgs);
			OnEventLogged(format);
		}

		private void LogWithContext(string format, RezolveContext context, params object[] otherFormatArgs)
		{
			string contextString = string.Format("[ Type: {0}, Name: {1}, With Scope? {2} ]", context.RequestedType, context.Name ?? "[null]", context.Scope != null ? "Yes" : "No");
			format = string.Format(format, contextString);
			Log(format, otherFormatArgs);
		}

		private void OnEventLogged(string eventMsg)
		{
			var evt = EventLogged;
			if (evt != null)
			{
				try
				{
					evt(this, new LogEventArgs(eventMsg));
				}
				catch (Exception) { }
			}
		}

		public override bool CanResolve(RezolveContext context)
		{
			var result = base.CanResolve(context);
			this.LogWithContext("CanResolve returning {{0}} for context {0}", context, result);
			return result;
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			OnEventLogged("CreateLifetimeScope called");
			return base.CreateLifetimeScope();
		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			var result = base.GetCompiledRezolveTarget(context);
			this.LogWithContext("GetCompiledRezolveTarget returning compiled target {{0}} for context {0}", context, result != null ? result.GetType().ToString() : "[null]");
			return result;
		}

		public override object Resolve(RezolveContext context)
		{
			try { 
			var result = base.Resolve(context);
			LogWithContext("Resolve returning {{0}} for context {0}", context, result != null ? string.Format("instance of {0}", result.GetType()) : "[null]");
			return result;
				}
			catch(Exception ex)
			{
				this.LogWithContext("Resolve failed for context {0}. Exception Type: {{0}}, Message: \"{{1}}\"",
					context, ex.GetType(), ex.Message);
				throw;
			}
		}

		public override bool TryResolve(RezolveContext context, out object result)
		{
			var bResult = base.TryResolve(context, out result);
			LogWithContext("TryResolve returning {{0}} for context {0}", context, bResult ? string.Format("true, with instance of {0}", result.GetType()) : "false");
			return bResult;
		}
	}
}
