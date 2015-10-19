using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// The core logger - 
	/// </summary>
	public class CallTrackingRezolverLogger : IRezolverLogger
	{
		private int _nextCallID = 1;

		/// <summary>
		/// acts as our flat list of calls.
		/// </summary>
		ConcurrentDictionary<int, LoggedRezolverCall> _calls = new ConcurrentDictionary<int, LoggedRezolverCall>();
		ConcurrentBag<string> _messages = new ConcurrentBag<string>();


		/// <summary>
		/// Each thread gets its own stack of calls.
		/// </summary>
		[ThreadStatic]
		Stack<LoggedRezolverCall> _currentCallStack = new Stack<LoggedRezolverCall>();

		public IEnumerable<LoggedRezolverCall> GetLoggedCalls()
		{
			return _calls.Values.ToArray().OrderBy(c => c.ID);
		}

		public void CallEnd(int callId)
		{
			//this call should be on the stack
			LoggedRezolverCall current = PopOrGetCall(callId);

			if (current != null)
				current.Ended();
		}

		private LoggedRezolverCall PopOrGetCall(int callId)
		{
			var current = _currentCallStack.Count != 0 ? _currentCallStack.Peek() : null;
			if (current != null && current.ID == callId)
			{
				_currentCallStack.Pop();
			}
			else
			{
				_calls.TryGetValue(callId, out current);
			}

			return current;
		}

		public LoggedRezolverCall GetCall(int callId)
		{
			LoggedRezolverCall call = null;
			_calls.TryGetValue(callId, out call);
			return call;
		}

		public void CallResult<TResult>(int callId, TResult result)
		{
			var call = PopOrGetCall(callId);
			if (call != null)
				call.Ended(result);
		}

		public int CallStart(object callee, object arguments, [CallerMemberName] string method = null)
		{
			int callID = _nextCallID++;
			var newCall = new LoggedRezolverCall(callID, callee, arguments, method, _currentCallStack.Count != 0 ? _currentCallStack.Peek() : null);
			_calls.TryAdd(callID, newCall);  //should think about handling when it can't
			return callID;
		}

		public void Exception(int callId, Exception ex)
		{
			var call = PopOrGetCall(callId);
			if (call != null)
				call.EndedWithException(ex);
		}

		public void Message(string message)
		{
			var currentCall = _currentCallStack.Count != 0 ? _currentCallStack.Peek() : null;
			if(currentCall != null)
			{
				currentCall.AddMessage(message);
			}
			else
			{
				_messages.Add(message);
			}
		}
	}
}
