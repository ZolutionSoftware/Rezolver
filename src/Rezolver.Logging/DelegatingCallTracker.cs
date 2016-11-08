// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Logging
{
	public interface ILoggingTarget
	{
		void Started(TrackedCall call);
		void Ended(TrackedCall call);
		void Result(TrackedCall call);
		void Exception(TrackedCall call);
		void Message(TrackedCallMessage message);
	}

	/// <summary>
	/// A call tracker that decorates another by echoing all logging events to one or more <see cref="ILoggingTarget"/> instances.
	/// 
	/// If you want immediate output for your Rezolver logs; or want otherwise to store them, then you'll most likely use this
	/// as your call tracker.
	/// In most cases you will simply use the <see cref="Default"/> instance and add <see cref="ILoggingTarget"/>
	/// </summary>
	public class DelegatingCallTracker : ICallTracker
	{
		private static readonly Lazy<DelegatingCallTracker> _default = new Lazy<DelegatingCallTracker>(() => new DelegatingCallTracker(CallTracker.Default));

		/// <summary>
		/// A single logging call tracker that wraps around the <see cref="CallTracker.Default"/> call tracker.
		/// </summary>
		public static DelegatingCallTracker Default
		{
			get
			{
				return _default.Value;
			}
		}

		private readonly ICallTracker _inner;
		private ConcurrentBag<ILoggingTarget> _loggingTargets;

		public DelegatingCallTracker(ICallTracker inner, params ILoggingTarget[] targets)
		{
			inner.MustNotBeNull(nameof(inner));
			_inner = inner;
			_loggingTargets = new ConcurrentBag<ILoggingTarget>(targets);
		}

		private void InvokeOnTargets(Action<ILoggingTarget> action)
		{
			var targets = _loggingTargets.ToArray();
			foreach (var target in targets)
			{
				action(target);
			}
		}

		private void GetCallAndInvokeOnTargets(long callID, Action<TrackedCall, ILoggingTarget> action)
		{
			var call = GetCall(callID);
			if (call != null) InvokeOnTargets(t => action(call, t));
		}

		public void AddTargets(params ILoggingTarget[] targets)
		{
			targets.MustNotBeNull(nameof(targets));
			foreach (var target in targets)
			{
				_loggingTargets.Add(target);
			}
		}

		public void CallEnd(long callID)
		{
			_inner.CallEnd(callID);
			GetCallAndInvokeOnTargets(callID, (c, t) => t.Ended(c));
		}

		public void CallResult<TResult>(long callID, TResult result)
		{
			_inner.CallResult(callID, result);
			GetCallAndInvokeOnTargets(callID, (c, t) => t.Result(c));
		}

		public long CallStart(object callee, object arguments, [CallerMemberName] string method = null)
		{
			var id = _inner.CallStart(callee, arguments, method);
			GetCallAndInvokeOnTargets(id, (c, t) => t.Started(c));
			return id;
		}

		public void Exception(long callID, Exception ex)
		{
			_inner.Exception(callID, ex);
			GetCallAndInvokeOnTargets(callID, (c, t) => t.Exception(c));
		}

		public TrackedCall GetCall(long callID)
		{
			return _inner.GetCall(callID);
		}

		public TrackedCallMessage Message(long callID, string message, MessageType messageType = MessageType.Information)
		{
			var msg = _inner.Message(callID, message, messageType);
			GetCallAndInvokeOnTargets(callID, (c, t) => t.Message(msg));
			return msg;
		}
	}
}
