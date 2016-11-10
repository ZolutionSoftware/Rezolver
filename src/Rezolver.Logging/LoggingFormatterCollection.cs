using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Provides string formatting capabilities (by implementing <see cref="IFormatProvider"/> and <see cref="ICustomFormatter"/>)
	/// through the use of instances of the <see cref="LoggingFormatter"/> object registered through the <see cref="AddFormatter(Type, LoggingFormatter)"/>
	/// function and its overloads.
	/// </summary>
	/// <seealso cref="System.IFormatProvider" />
	/// <seealso cref="System.ICustomFormatter" />
	/// <remarks>This class allows you to customise how objects are formatted in messages and other string values for <see cref="TrackedCall"/>
	/// instances.</remarks>
	public sealed class LoggingFormatterCollection : IFormatProvider, ICustomFormatter
	{
		/// <summary>
		/// Gets the default logging formatter collection that's used by default in <see cref="CallTracker"/> and
		/// <see cref="TrackedCall"/> instances.
		/// </summary>
		public static LoggingFormatterCollection Default { get; }

		static LoggingFormatterCollection()
		{
			Default = new LoggingFormatterCollection();
			Default.AddFormatter(new Formatters.ExceptionFormatter<Exception>());
		}

		private readonly LoggingFormatterCollection _innerCollection;


		private readonly ConcurrentDictionary<Type, LoggingFormatter> _formatters;
		/// <summary>
		/// Initialises a new instance of the <see cref="LoggingFormatterCollection" /> class, optionally using the passed <paramref name="source" /> as
		/// the starting point for new registrations.
		/// </summary>
		/// <param name="innerCollection">The inner collection to be used if this collection cannot locate a <see cref="LoggingFormatter"/> for a given type.
		/// 
		/// Using parameter, it's possible to extend an existing collection on an ad-hoc basis without modifying it.  Typically, for example, a collection
		/// might be initialised with the <see cref="Default"/> collection passed as the argument to this parameter, thus allowing a component to use the 
		/// default collection of formatters, but overriding and extending them as required without replacing any other formatters that are actually required
		/// by other components.</param>
		public LoggingFormatterCollection(LoggingFormatterCollection innerCollection = null)
		{
			_formatters = new ConcurrentDictionary<Type, LoggingFormatter>();
			//have to make sure all the inner collections are unique
			if(innerCollection != null)
			{
				//we can copy the reference across straight away, if an exception is thrown below, then the 
				//construction fails and it's not used anyway.
				_innerCollection = innerCollection;
				List<LoggingFormatterCollection> nestedCollections = new List<LoggingFormatterCollection>();
				
				while(innerCollection != null)
				{
					if (nestedCollections.Contains(innerCollection))
						throw new ArgumentException("Circular loop detected in innerCollection", nameof(innerCollection));
					nestedCollections.Add(innerCollection);
					innerCollection = innerCollection._innerCollection;
				}
			}
		}

		/// <summary>
		/// Adds the formatter to the collection, associating it with the given type.
		/// 
		/// If a formatter already exists for this type, it is replaced.
		/// </summary>
		/// <typeparam name="TObject">The type of object for which this formatter will be used to produce formatted strings.</typeparam>
		/// <param name="formatter">The formatter.</param>
		public void AddFormatter<TObject>(LoggingFormatter formatter)
		{
			try
			{
				AddFormatter(typeof(TObject), formatter);
			}
			catch (ArgumentNullException)
			{
				throw;
			}
			catch (ArgumentException aex)
			{
				throw new InvalidOperationException(string.Format("The type {0} cannot be registered", typeof(TObject)), aex);
			}
		}

		public void AddFormatter<TObject>(LoggingFormatter<TObject> formatter)
		{
			AddFormatter(typeof(TObject), formatter);
		}

		public void AddFormatter(Type objectType, LoggingFormatter formatter)
		{
			objectType.MustNotBeNull(nameof(objectType));
			formatter.MustNotBeNull(nameof(formatter));
			objectType.MustNot(t => t == typeof(IFormattable), "Cannot explicitly add a formatter for the IFormattable type");

			_formatters[objectType] = formatter;
		}

		/// <summary>
		/// Produces a string representation of the arguments provided using this object (and the
		/// <see cref="LoggingFormatter"/> instances that have been registered within it) as a custom formatter.
		/// </summary>
		/// <param name="formatString">The format string.  If format placeholders are present, then the <paramref name="args"/> must contain
		/// values for each of them.</param>
		/// <param name="args">The values to be formatted into the <paramref name="formatString"/>, if format placeholders are present within it.</param>
		/// <remarks>This is an implementation of the traditional string formatting pattern, whereby you provide a string with optional
		/// formatting placeholders along with the values which are to be formatted into the output string.  The function will invoke
		/// <see cref="string.Format(IFormatProvider, string, object[])"/> passing itself as the <see cref="IFormatProvider"/> to the first parameter.</remarks>
		public string Format(string formatString, params object[] args)
		{
			return string.Format(this, formatString, args);
		}

		/// <summary>
		/// Formats the specified formattable object using the custom formatter defined by this
		/// type, leveraging the <see cref="LoggingFormatter"/> objects contained within this collection
		/// to produce the string.
		/// </summary>
		/// <param name="format">The formattable object.</param>
		/// <remarks>This function is useful when you want to format a log message from a .Net interpolated
		/// string using the <see cref="LoggingFormatter"/> objects that have been registered in this collection.
		/// 
		/// The only way to invoke it directly when using an interpolated string, however, is to reference the 
		/// <paramref name="format"/> parameter by name:</remarks>
		/// <example>
		/// <para>As described in the remarks section, this overload is most suited for when you wish to format an interpolated
		/// string leveraging this class' implementation of <see cref="ICustomFormatter"/> and <see cref="IFormatProvider"/>.
		/// In order to do that, however, you cannot simply pass the interpolated string as an argument, you must either first
		/// capture the interpolated string in an <see cref="IFormattable"/> variable, or specify an argument for the
		/// <paramref name="format"/> parameter by name:</para>
		/// <code>
		/// //resolving the overload using an explicitly typed IFormattable
		/// IFormattable f = $&quot;Hello { value }&quot;;
		/// collection.Format(f);</code>
		/// <code>
		/// //specifying the format parameter by name
		/// collection.Format(format: $&quot;Hello { value }&quot;);</code></example>
		public string Format(IFormattable format)
		{
			return format.ToString(null, this);
		}

		/// <summary>
		/// Formats the given object using a <see cref="LoggingFormatter" /> registered in this collection by a type which is
		/// equal to, an interface or base of, or an open generic of the object's type.
		/// 
		/// If no <see cref="LoggingFormatter"/> has been registered with a compatible type, but object implements the <see cref="IFormattable"/> interface,
		/// then its implementation of the <see cref="IFormattable.ToString(string, IFormatProvider)"/> will be used, optionally passing through the 
		/// <paramref name="format"/> and <paramref name="formatProvider"/> arguments if provided.
		/// 
		/// Otherwise, the <see cref="object.ToString"/> function is called.
		/// </summary>
		/// <param name="obj">The object to be formatted.</param>
		/// <param name="format">Optional format string that can be used to customise how the object is output to the string.
		/// Either be passed to the handling formatter's <see cref="LoggingFormatter.Format(object, string)"/> function, 
		/// or the <see cref="IFormattable.ToString(string, IFormatProvider)"/> function if the object is <see cref="IFormattable"/>.</param>
		/// <param name="formatProvider">Optional. Only used if no compatible <see cref="LoggingFormatter"/> is registered in this collection
		/// and the object implements the <see cref="IFormattable"/> interface.  In this case, this argument will be passed through to the
		/// object's implementation of the <see cref="IFormattable.ToString(string, IFormatProvider)"/> function.</param>
		public string Format(object obj, string format = null, IFormatProvider formatProvider = null)
		{
			if (obj == null)
				return "[null]";

			string toReturn;
			foreach (var formatter in GetFormatters(obj))
			{
				toReturn = formatter.Format(obj, format);
				if (toReturn != null)
					return toReturn;
			}
			//note: we prevent this collection from being passed as the formatProvider into this IFormattable
			//to avoid infinite recursion.
			if (obj is IFormattable)
				return ((IFormattable)obj).ToString(format, formatProvider != this ? formatProvider : null);
			else
				return obj.ToString();
		}

		/// <summary>
		/// Gets the formatters registered in this collection which are compatible with the given object.
		/// </summary>
		/// <param name="obj">The object.</param>
		private IEnumerable<LoggingFormatter> GetFormatters(object obj)
		{
			if (obj != null)
			{
				LoggingFormatter formatter;
				foreach (var type in GetTypeSearchList(obj))
				{
					if ((formatter = GetFormatterForTypeExact(type)) != null)
						yield return formatter;
				}
			}
		}

		private LoggingFormatter GetFormatterForTypeExact(Type type)
		{
			//exact type match can come from this collection or from the inner collection (or from its inner collection, ad nauseam)
			LoggingFormatter toReturn;
			if (!_formatters.TryGetValue(type, out toReturn) && _innerCollection != null)
				toReturn = _innerCollection.GetFormatterForTypeExact(type);
			return toReturn;
		}

		private IEnumerable<Type> GetTypeSearchList(object obj)
		{
			if (obj == null)
			{
				yield return typeof(object);
				yield break;
			}
			//the type, then any interfaces.
			//the base, then any interfaces.
			//for each type, if it's a generic type, then we grab the generic type definition
			foreach (var t in ExplodeType(obj.GetType()))
			{
				yield return t;
			}
		}

		private IEnumerable<Type> ExplodeType(Type t)
		{
			yield return t;
			if (TypeHelpers.IsGenericType(t))
				yield return t.GetGenericTypeDefinition();
			foreach (var iType in TypeHelpers.GetInterfaces(t))
			{
				foreach (var tt in ExplodeType(iType))
				{
					yield return tt;
				}
			}
		}

		/// <summary>
		/// Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.
		/// </summary>
		/// <param name="format">A format string containing formatting specifications.</param>
		/// <param name="arg">An object to format.</param>
		/// <param name="formatProvider">An object that supplies format information about the current instance.</param>
		/// <returns>The string representation of the value of <paramref name="arg" />, formatted as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
		string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider)
		{
			return Format(arg, format, formatProvider);
		}

		/// <summary>
		/// Returns an object that provides formatting services for the specified type.
		/// </summary>
		/// <param name="formatType">An object that specifies the type of format object to return.</param>
		/// <returns>An instance of the object specified by <paramref name="formatType" />, if the <see cref="T:System.IFormatProvider" /> implementation can supply that type of object; otherwise, null.</returns>
		object IFormatProvider.GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}
	}
}
