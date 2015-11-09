using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Diagnostics
{
	internal class TrackingUtils
	{
		private static readonly object _locker = new object();
		private static Dictionary<string, int> _ids = new Dictionary<string, int>();
		public static int NextID(string type = null)
		{
			type = type ?? string.Empty;
			lock (_locker)
			{
				int toReturn = 0;
				_ids.TryGetValue(type, out toReturn);
				_ids[type] = toReturn = ++toReturn;
				return toReturn;
			}
		}

		public static int NextRezolverID()
		{
			return NextID("rezolver");
		}

		public static int NextBuilderID()
		{
			return NextID("builder");
		}
	}
}
