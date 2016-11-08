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
		public TrackedCall ParentCall { get; }
		public string Text { get; }
		public MessageType Type { get; }
		public DateTime Timestamp { get; }

		internal TrackedCallMessage(TrackedCall parent, string text, MessageType type = MessageType.Information, DateTime? timestamp = null)
		{
			ParentCall = parent;
			Text = text;
			Type = type;
			Timestamp = timestamp ?? DateTime.UtcNow;
		}
    }
}
