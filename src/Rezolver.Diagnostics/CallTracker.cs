// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Diagnostics
{
	/// <summary>
	/// The root tracker class - creates a graph of all calls to objects as they happen.
	/// </summary>
	public class CallTracker : ICallTracker
	{
		private static readonly Lazy<CallTracker> _default = new Lazy<CallTracker>(() => new CallTracker());

		/// <summary>
		/// The default CallTracker instance - used for application-wide rezolver tracking.
		/// </summary>
		public static CallTracker Default
		{
			get
			{
				return _default.Value;
			}
		}

		private int _nextCallID = 1;

		/// <summary>
		/// acts as our indexed set of completed calls
		/// </summary>
		ConcurrentDictionary<int, TrackedCall> _calls = new ConcurrentDictionary<int, TrackedCall>();
		/// <summary>
		/// ongoing calls are kept in here, then migrated when they're completed
		/// </summary>
		ConcurrentDictionary<int, TrackedCall> _callsInProgress = new ConcurrentDictionary<int, TrackedCall>();
		ConcurrentBag<string> _messages = new ConcurrentBag<string>();

		private bool _retainCompletedCalls;
		/// <summary>
		/// Controls whether calls that are completed are retained after they are finished.
		/// 
		/// The default is false, which ensures that call tracking can be enabled without placing a potentially large memory overhead
		/// on the application.
		/// 
		/// When false, the GetCompletedCalls method will always return nothing.
		/// </summary>
		public bool RetainCompletedCalls
		{
			get
			{
				return _retainCompletedCalls;
			}
			set
			{
				_retainCompletedCalls = value;
				if (!_retainCompletedCalls)
				{
					_calls.Clear();
				}
			}
		}

		/// <summary>
		/// Each thread gets its own stack of calls.
		/// </summary>
		[ThreadStatic]
		Stack<TrackedCall> _currentCallStack = new Stack<TrackedCall>();

		public TrackedCallGraph GetCompletedCalls()
		{
			var all = _calls.ToArray();
			return new TrackedCallGraph(all.Select(k => k.Value).OrderBy(c => c.Timestamp));
		}

		public void CallEnd(int callID)
		{
			//this call should be on the stack
			TrackedCall call = PopOrGetIncompleteCall(callID);

			if (call != null)
			{
				call.Ended();
				CallComplete(call);
			}
		}

		/// <summary>
		/// removes a call from the in-progress dictionary and, if the 
		/// call is a root call (no parent), then imports the call and all its
		/// children into the 'completed' dictionary.
		/// </summary>
		/// <param name="completed"></param>
		private void CallComplete(TrackedCall completed)
		{
			int counter = 5;
			TrackedCall removed = null;
			while (counter-- > 0)
			{
				if (_callsInProgress.TryRemove(completed.ID, out removed))
					break;
			}

			if (_retainCompletedCalls && removed != null && completed.Parent == null)
			{

				counter = 5;
				while (counter-- > 0)
				{
					if (_calls.TryAdd(completed.ID, completed))
						break;
        }
			}
		}

		private TrackedCall PopOrGetIncompleteCall(int callID)
		{
			var current = _currentCallStack.Count != 0 ? _currentCallStack.Peek() : null;
			if (current != null && current.ID == callID)
			{
				_currentCallStack.Pop();
			}
			else
			{
				_callsInProgress.TryGetValue(callID, out current);
			}

			return current;
		}

		public TrackedCall GetCall(int callID)
		{
			TrackedCall call = null;
			if(!_callsInProgress.TryGetValue(callID, out call))
				_calls.TryGetValue(callID, out call);
			return call;
		}

		public void CallResult<TResult>(int callID, TResult result)
		{
			var call = PopOrGetIncompleteCall(callID);
			if (call != null)
			{
				call.Ended(result);
				CallComplete(call);
			}
		}

		public int CallStart(object callee, object arguments, [CallerMemberName] string method = null)
		{
			int callID = _nextCallID++;
			var newCall = new TrackedCall(callID, callee, arguments, method, _currentCallStack.Count != 0 ? _currentCallStack.Peek() : null);
			_callsInProgress.TryAdd(callID, newCall);  //should think about handling when it can't
			_currentCallStack.Push(newCall);
			return callID;
		}

		public void Exception(int callID, Exception ex)
		{
			var call = PopOrGetIncompleteCall(callID);
			if (call != null)
			{
				call.EndedWithException(ex);
				CallComplete(call);
			}
		}

		public void Message(int callID, string message)
		{
			var call = GetCall(callID);
			if(call != null)
				call.AddMessage(message);

		}
	}
}
