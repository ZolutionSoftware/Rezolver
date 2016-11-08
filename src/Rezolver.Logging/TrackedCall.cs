// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;

namespace Rezolver.Logging
{
	/// <summary>
	/// Represents a call to a method on an object that has been tracked by an ICallTracker.
	/// 
	/// Note that the class is not thread-safe.  You should only enumerate/evaluate this object from your
	/// code when it's complete.
	/// </summary>
	public class TrackedCall
	{
		/// <summary>
		/// Used when reading objects to turn them into dictionaries.
		/// </summary>
		protected class ObjectMemberValue
		{
			public string Name { get; }
			public object Value { get; }

			public ObjectMemberValue(object obj, MemberInfo member)
			{
				Name = member.Name;
				Func<object, MemberInfo, object> accessor = null;
				if (member is FieldInfo)
				{
					accessor = GetValueFromField;
				}
				else if (member is PropertyInfo)
				{
					accessor = GetValueFromProperty;
				}

				if (accessor != null)
				{
					try
					{
						Value = accessor(obj, member);
					}
					catch (Exception ex)
					{
						Value = $"[Exception ({ ex.GetType() })]: { ex.Message }";
					}
				}
			}

			private static object GetValueFromProperty(object o, MemberInfo p)
			{
				return ((PropertyInfo)p).GetValue(o);
			}

			private static object GetValueFromField(object o, MemberInfo f)
			{
				return ((FieldInfo)f).GetValue(o);
			}
		}

		public string Callee { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance has a result - can only be set to true by calling the <see cref="Ended(object)"/> and passing 
		/// </summary>
		/// <value><c>true</c> if this instance has result; otherwise, <c>false</c>.</value>
		public bool HasResult { get; private set; }
		public string Result { get; private set; }
		public Dictionary<string, string> Arguments { get; private set; }

		public string Method { get; private set; }

		public TrackedCall Parent { get; private set; }
		public long ID { get; private set; }

		public DateTime Timestamp { get; private set; }

		public DateTime? Completed { get; private set; }

		/// <summary>
		/// A dictionary representing additional data for this call.
		/// </summary>
		/// <value>The data.</value>
		public Dictionary<string, object> Data { get; }

		private readonly BlockingCollection<TrackedCallMessage> _messages;
		public IEnumerable<TrackedCallMessage> Messages { get { return _messages.AsReadOnly(); } }

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

		public BlockingCollection<TrackedCall> ChildCalls { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TrackedCall"/> class.
		/// </summary>
		/// <param name="id">The unique ID of the call</param>
		/// <param name="callee">The object on which the method was called.</param>
		/// <param name="arguments">The arguments passed to the method.  Expected to be an object whose properties/fields reflect the arguments (name and type)</param>
		/// <param name="method">The method.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="data">Additional data to be attached to this call, will be used to initialise the <see cref="Data"/> dictionary from the object's public 
		/// properties and fields, where the property/field name will be used as the key, and the property/field value used as the value.</param>
		public TrackedCall(long id, object callee, object arguments, string method, TrackedCall parent = null, object data = null)
		{
			_messages = new BlockingCollection<TrackedCallMessage>();
			Data = ExplodeObjectMembersToObjectDictionary(data);
			Timestamp = DateTime.UtcNow;
			ChildCalls = new BlockingCollection<TrackedCall>();
			Parent = parent;
			Method = method;
			ID = id;
			Callee = FormatObjectString(callee);
			Arguments = ExplodeObjectMembersToStringDictionary(arguments);
			if (Parent != null)
				Parent.ChildCalls.Add(this);
		}

		

		public void Ended(object result = null, bool hasResult = false)
		{
			if (Completed == null)
				Completed = DateTime.UtcNow;

			if (result != null && Result == null)
			{
				if (hasResult)
				{
					Result = FormatObjectString(result, true);
					HasResult = true;
				}
				//HasResult is used to differentiate between void functions and those
				//which return null.
			}
		}

		public void EndedWithException(Exception ex)
		{
			if (Completed == null)
				Completed = DateTime.UtcNow;

			Exception = $"Exception of type {ex.GetType()} occurred: {ex.Message}";
		}

		public TrackedCallMessage AddMessage(string message, MessageType messageType = MessageType.Information, DateTime? timestamp = null)
		{
			var toReturn = new TrackedCallMessage(this, message, messageType, timestamp);
			_messages.Add(toReturn);
			return toReturn;
		}

		private IEnumerable<ObjectMemberValue> ExplodeObjectMembers(object obj)
		{
			if (obj == null)
				return Enumerable.Empty<ObjectMemberValue>();

			return obj.GetType().GetInstanceProperties().PubliclyReadable().Cast<MemberInfo>().Concat(
				obj.GetType().GetInstanceFields().Public().Cast<MemberInfo>()).Select(m => new ObjectMemberValue(obj, m));
		}

		private Dictionary<string, object> ExplodeObjectMembersToObjectDictionary(object obj)
		{
			return ExplodeObjectMembers(obj).ToDictionary(mv => mv.Name, mv => mv.Value);
		}

		private Dictionary<string, string> ExplodeObjectMembersToStringDictionary(object obj)
		{
			return ExplodeObjectMembers(obj).ToDictionary(mv => mv.Name, mv => FormatObjectString(mv.Value, true));
		}

		/// <summary>
		/// Formats a string for the passed object.
		/// 
		/// IMPORTANT to overriders: this function can be called by the constructor if arguments are present on the tracked call; so if you 
		/// override this method you should ensure that you don't rely on 
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="includeValue">if set to <c>true</c> [include value].</param>
		/// <returns>System.String.</returns>
		protected virtual string FormatObjectString(object value, bool includeValue = false)
		{
			if (value == null)
				return "<null>";

			if (includeValue)
			{
				try
				{
					return $"({value.GetType()}): {value}";
				}
				catch (Exception ex)
				{
					return $"[Exception ({ ex.GetType() })]: { ex.Message }";
				}
			}
			else
				return $"({value.GetType()})";
		}

		protected virtual string FormatObjectValueString(object value)
		{
			return value.ToString();
		}
	}
}
