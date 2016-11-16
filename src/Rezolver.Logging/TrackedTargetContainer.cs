﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Logging
{
	public class TrackedTargetContainer : TargetContainer
	{
		private readonly int _id = TrackingUtils.NextID<TrackedTargetContainer>();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		public ICallTracker Logger { get; private set; }
		/// <summary>
		/// Creates a new instance that wraps around the passed IRezolveTargetContainer.
		/// </summary>
		/// <param name="logger">The logger that is to receive logging calls from this instance.</param>
		public TrackedTargetContainer(ICallTracker logger)
		{
			Logger = logger;
		}

		public override ITarget Fetch(Type type)
		{
			return base.Fetch(type);
		}

		public override void Register(ITarget target, Type type = null)
		{

			Logger.TrackCall(this, callID =>
			{
				base.Register(target, type);
				Logger.Message(callID, MessageType.Information, format: $"{ target } has been registered for type { type ?? target.DeclaredType })");
			});
		}
	}
}
