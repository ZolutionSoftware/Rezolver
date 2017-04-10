using Rezolver.Compilation;
using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class RezolverLoggerTests
	{
		private class TestRezolverBase : IContainer
		{
			public virtual ITargetContainer Targets
			{
				get
				{
					return null;
				}
			}

			public virtual ITargetCompiler Compiler
			{
				get
				{
					return null;
				}
			}

			public virtual bool CanResolve(IResolveContext context)
			{
				return false;
			}

			public virtual IContainerScope CreateScope()
			{
				return null;
			}

			public virtual ICompiledTarget FetchCompiled(IResolveContext context)
			{
				return null;
			}

			public virtual object GetService(Type serviceType)
			{
				return null;
			}

			public virtual object Resolve(IResolveContext context)
			{
				return null;
			}

			public virtual bool TryResolve(IResolveContext context, out object result)
			{
				result = null;
				return false;
			}
		}

		private class TestLogger : ICallTracker
		{
			private long _lastReqID = 0;

			public ObjectFormatterCollection MessageFormatter
			{
				get
				{
					return ObjectFormatterCollection.Default;
				}
			}

			public long RequestCount
			{
				get
				{
					return _lastReqID;
				}
			}
			public void CallEnd(long reqId)
			{
				Debug.WriteLine($"CallEnd called for reqId {reqId}");
			}

			public void CallResult<TResult>(long reqId, TResult result)
			{
                Debug.WriteLine($"CallResult called for reqId {reqId}, result: {(result == null ? "null" : result.ToString())}");
			}

			public long CallStart(object callee, object arguments, [CallerMemberName] string method = null, object data = null)
			{

				long thisReqId = ++_lastReqID;
                Debug.WriteLine($"CallStart called on object {callee} for method {method}. Arguments? {(arguments != null ? "Yes" : "No")}.  ReqId: {thisReqId}");
				return thisReqId;
			}

			public void Exception(long reqId, Exception ex)
			{
                Debug.WriteLine($"Exception called for reqId {reqId}.  Exception: { ex?.Message }");
			}

			public TrackedCall GetCall(long callID)
			{
				throw new NotImplementedException();
			}

			public TrackedCall GetCurrentCall()
			{
				throw new NotImplementedException();
			}

			public TrackedCallMessage Message(long callID, MessageType messageType, IFormattable format)
			{
				var msg = format.ToString(null, MessageFormatter);
                Debug.WriteLine(msg);
				return new TrackedCallMessage(new TrackedCall(callID, null), messageType, msg, DateTime.UtcNow);
			}

			public TrackedCallMessage Message(long callID, MessageType messageType, string message)
			{
                Debug.WriteLine(message);
				return new TrackedCallMessage(new TrackedCall(callID, null), messageType, message, DateTime.UtcNow);
			}

			public TrackedCallMessage Message(long callID, MessageType messageType, string messageFormat, params object[] formatArgs)
			{
				var msg = MessageFormatter.Format(messageFormat, formatArgs);
                Debug.WriteLine(msg);
				return new TrackedCallMessage(new TrackedCall(callID, null), messageType, msg, DateTime.UtcNow);
			}
		}

		private class RequiresInt
		{
			public int IntValue { get; private set; }
			public RequiresInt(int intValue)
			{
				IntValue = intValue;
			}
		}

		[Fact]
		public void ShouldRecordRezolverCall()
		{
			var logger = new TestLogger();
			var loggingRezolver = new TrackedContainer(logger);

			try
			{
				loggingRezolver.Resolve(typeof(int));
			}
			catch (InvalidOperationException) { /* expected - no registrations */ }

			Assert.NotEqual(0, logger.RequestCount);
		}

		[Fact]
		public void ShouldWorkWithDefaultRezolver()
		{
			//could use a more elaborate test here, but the most important thing is that the rezolve operation 
			//works and that nothing throws an exception.  I'm happy with that as a test.  For now.

			//If bugs do start to show up 
			var logger = new TestLogger();
			var loggingRezolver = new TrackedContainer(logger);

			loggingRezolver.RegisterObject(10);
			loggingRezolver.RegisterType<RequiresInt>();

			var result = loggingRezolver.Resolve<RequiresInt>();

			using (var scope = loggingRezolver.CreateScope())
			{
				var result2 = scope.Resolve<RequiresInt>();
			}
		}
	}
}
