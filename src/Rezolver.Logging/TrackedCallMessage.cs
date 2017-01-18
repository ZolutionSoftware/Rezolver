// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Class TrackedCallMessage.
	/// </summary>
	public class TrackedCallMessage
	{
		public TrackedCall Call { get; }
		public string Text { get; }
		public MessageType Type { get; }
		public DateTime Timestamp { get; }

		public TrackedCallMessage(TrackedCall call, MessageType type, string text, DateTime timestamp)
		{
			Call = call;
			Type = type;
			Text = text;
			Timestamp = timestamp;
		}
	}
}
