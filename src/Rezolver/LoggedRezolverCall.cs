using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// Represents a call that has been logged by an IRezolverLogger.
	/// </summary>
	public class LoggedRezolverCall
	{
		public LoggedRezolverCall(int id, object callee, object arguments, string method, LoggedRezolverCall parent = null)
		{
			Callee = FormatObjectString(callee);
			Arguments = GetArgumentsStrings(arguments).ToArray();
			Timestamp = DateTime.UtcNow;
			ChildCalls = new List<LoggedRezolverCall>();
			Parent = parent;
			ID = id;
			Method = method;
			_messages = new List<string>();

			if (Parent != null)
				Parent.ChildCalls.Add(this);
		}

		public string Callee { get; private set; }
		public string Result { get; private set; }
		public KeyValuePair<string, string>[] Arguments { get; private set; }

		public string Method { get; private set; }

		public LoggedRezolverCall Parent { get; private set; }
		public int ID { get; private set; }

		public DateTime Timestamp { get; private set; }

		public DateTime? Completed { get; private set; }

		private readonly List<string> _messages;
		public IEnumerable<string> Messages { get { return _messages; } }

		public string Exception { get; private set; }

		public TimeSpan? Duration
		{
			get
			{
				if (Completed != null)
					return Completed.Value - Timestamp;

				return null;
			}
		}

		public List<LoggedRezolverCall> ChildCalls { get; private set; }

		public void Ended(object result = null)
		{
			if (Completed == null)
				Completed = DateTime.UtcNow;

			if (result != null && Result == null)
				Result = FormatObjectString(result);
		}

		public void EndedWithException(Exception ex)
		{
			if (Completed == null)
				Completed = DateTime.UtcNow;

			Exception = $"Exception of type {ex.GetType()} occurred: {ex.Message}";
		}

		public void AddMessage(string message)
		{
			_messages.Add(message);
		}

		private IEnumerable<KeyValuePair<string, string>> GetArgumentsStrings(object arguments)
		{
			if (arguments == null)
				return new KeyValuePair<string, string>[0];

			return arguments.GetType().GetInstanceProperties().PubliclyReadable().Select(p =>
			{
				try
				{
					return new KeyValuePair<string, string>(p.Name, FormatObjectString(p.GetValue(arguments, new object[0]), true));
				}
				catch (Exception ex)
				{
					return new KeyValuePair<string, string>(p.Name, $"(Exception {ex.GetType()} occurred");
				}
			}).Concat(arguments.GetType().GetInstanceFields().Public().Select(f =>
			{
				try
				{
					return new KeyValuePair<string, string>(f.Name, FormatObjectString(f.GetValue(arguments), true));
				}
				catch (Exception ex)
				{
					return new KeyValuePair<string, string>(f.Name, $"(Exception {ex.GetType()} occurred");
				}
			}));
		}

		private string FormatObjectString(object callee, bool includeValue = false)
		{
			if (callee == null)
				return "<null>";

			if (includeValue)
				return $"({callee.GetType()}): {callee}";
			else
				return $"({callee.GetType()})";
		}
	}
}
