// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Concurrent;
using Rezolver.Logging.Formatters;

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
		bool _initialised;

		/// <summary>
		/// Used when reading objects to turn them into dictionaries.
		/// </summary>
		protected class ObjectMemberValue
		{
			public string Name { get; }
			public object Value { get; }
			public Type Type { get; }

			public ObjectMemberValue(object obj, MemberInfo member)
			{
				Name = member.Name;
				//initialise it to *something*!
				Type = typeof(object);
				Func<object, MemberInfo, object> accessor = null;
				if (member is FieldInfo)
				{
					accessor = GetValueFromField;
					Type = ((FieldInfo)member).FieldType;
				}
				else if (member is PropertyInfo)
				{
					accessor = GetValueFromProperty;
					Type = ((PropertyInfo)member).PropertyType;
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

		public LoggingFormatterCollection MessageFormatter { get; }

		public string Callee { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance has a result - can only be set to true by calling the <see cref="Ended(object)"/> and passing 
		/// </summary>
		/// <value><c>true</c> if this instance has result; otherwise, <c>false</c>.</value>
		public bool HasResult { get; private set; }
		public string Result { get; private set; }
		public Dictionary<string, string> Arguments { get; private set; }

		public string Method { get; private set; }

		public TrackedCall Parent { get; }
		public long ID { get; }

		public DateTime Timestamp { get; }

		public DateTime? Completed { get; private set; }

		/// <summary>
		/// A dictionary representing additional data for this call.
		/// </summary>
		/// <value>The data.</value>
		public Dictionary<string, object> Data { get; private set; }

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
		/// Occurs when a message is added to this call through either <see cref="AddMessage(MessageType, IFormattable)"/> 
		/// or <see cref="AddMessage(MessageType, string, object[])"/>.
		/// </summary>
		public event EventHandler<TrackedCallMessage> MessageAdded;

		/// <summary>
		/// Creates a new instance of the <see cref="TrackedCall" /> class.
		/// 
		/// After creation, the call must be initialised with a call to <see cref="Init(object, object, string, object)"/>
		/// </summary>
		/// <param name="id">The unique ID of the call</param>
		/// <param name="parent">The parent.</param>
		/// <param name="data">Additional data to be attached to this call, will be used to initialise the <see cref="Data" /> dictionary from the object's public
		/// properties and fields, where the property/field name will be used as the key, and the property/field value used as the value.</param>
		/// <param name="messageFormatters">The formatters to be used by this call or its message objects when formatting any objects into strings.  If not provided,
		/// then the class defaults to the <see cref="LoggingFormatterCollection.Default"/>.</param>
		public TrackedCall(long id, TrackedCall parent, LoggingFormatterCollection messageFormatter = null)
		{
			ID = id;
			Parent = parent;
			Timestamp = DateTime.UtcNow;
			MessageFormatter = messageFormatter ?? LoggingFormatterCollection.Default;
			_messages = new BlockingCollection<TrackedCallMessage>();
			ChildCalls = new BlockingCollection<TrackedCall>();
		}

		/// <summary>
		/// Initializes this <see cref="TrackedCall"/> instance with the given callee, arguments, method name and data.
		/// 
		/// The <paramref name="callee"/> and <paramref name="arguments"/> values will be converted to strings using the 
		/// <see cref="MessageFormatter"/> that was passed to this call on construction.
		/// 
		/// It is only after this call that the call is considered 'live' and will be present in its <see cref="Parent"/>'s 
		/// <see cref="TrackedCall.ChildCalls"/> collection.
		/// </summary>
		/// <param name="callee">The object on which the <paramref name="method"/> has been called.</param>
		/// <param name="arguments">An object whose properties contain the arguments that were passed to the method.  The name of the property should equal the 
		/// name of the parameter, and the value of the property should be the argument.</param>
		/// <param name="method">The name of the method that was called on the <paramref name="callee"/></param>
		/// <param name="data">An optional object with additional data that is to be attached to the call.
		/// 
		/// The publicly readable properties of this object, if provided, are turned into a dictionary (the property name being used as the key, and the
		/// property value the value)</param> which is then available through the <see cref="Data"/> property.
		/// <exception cref="InvalidOperationException">This TrackedCall has already been initialised</exception>
		public virtual void Init(object callee, object arguments, string method, object data)
		{
			if (_initialised) throw new InvalidOperationException("This TrackedCall has already been initialised");

			Data = ExplodeObjectMembersToObjectDictionary(data);
			Method = method;
			Callee = MessageFormatter.Format(callee);
			Arguments = ExplodeObjectMembersToStringDictionary(arguments);
			if (Parent != null)
				Parent.ChildCalls.Add(this);
		}

		protected virtual void OnMessageAdded(TrackedCallMessage msg)
		{
			MessageAdded?.Invoke(this, msg);
		}

		public void Ended(object result = null, bool hasResult = false)
		{
			if (Completed == null)
				Completed = DateTime.UtcNow;

			if (result != null && Result == null)
			{
				if (hasResult)
				{
					Result = MessageFormatter.Format(result);
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

			Exception = MessageFormatter.Format(ex, ExceptionFormatter<Exception>.Format_WithStackTrace);
		}

		/// <summary>
		/// Adds the message.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="formatString">The format string.</param>
		/// <param name="formatArgs">The format arguments.</param>
		/// <returns>TrackedCallMessage.</returns>
		public TrackedCallMessage AddMessage(MessageType messageType, string formatString, params object[] formatArgs)
		{
			var toReturn = new TrackedCallMessage(this, messageType, MessageFormatter.Format(formatString, formatArgs), DateTime.UtcNow);
			_messages.Add(toReturn);
			OnMessageAdded(toReturn);
			return toReturn;
		}

		public TrackedCallMessage AddMessage(MessageType messageType, IFormattable format)
		{
			var toReturn = new TrackedCallMessage(this, messageType, MessageFormatter.Format(format), DateTime.UtcNow);
			_messages.Add(toReturn);
			OnMessageAdded(toReturn);
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
			return ExplodeObjectMembers(obj).ToDictionary(mv => mv.Name, mv => MessageFormatter.Format(format: $"{ mv.Value } ({mv.Type})"));
		}
	}
}
