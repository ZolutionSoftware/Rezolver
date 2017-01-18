// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// Provides string formatting capabilities (by implementing <see cref="IFormatProvider"/> and <see cref="ICustomFormatter"/>)
	/// through the use of instances of the <see cref="ObjectFormatter"/> object registered through the <see cref="AddFormatter(Type, ObjectFormatter)"/>
	/// function and its overloads.
	/// </summary>
	/// <seealso cref="System.IFormatProvider" />
	/// <seealso cref="System.ICustomFormatter" />
	/// <remarks>This class allows you to customise how objects are formatted in messages and other string values for <see cref="TrackedCall"/>
	/// instances.</remarks>
	public sealed class ObjectFormatterCollection : IFormatProvider, ICustomFormatter
	{
		/// <summary>
		/// Gets the default logging formatter collection that's used by default in <see cref="CallTracker"/> and
		/// <see cref="TrackedCall"/> instances.
		/// </summary>
		public static ObjectFormatterCollection Default { get; }

		static ObjectFormatterCollection()
		{
			Default = new ObjectFormatterCollection();
			Default.AddFormattersFromAssembly(TypeHelpers.GetAssembly(typeof(ObjectFormatterCollection)));
		}

		private readonly ObjectFormatterCollection _innerCollection;


		private readonly ConcurrentDictionary<Type, ObjectFormatter> _formatters;
		/// <summary>
		/// Initialises a new instance of the <see cref="ObjectFormatterCollection" /> class, optionally using the passed <paramref name="source" /> as
		/// the starting point for new registrations.
		/// </summary>
		/// <param name="innerCollection">The inner collection to be used if this collection cannot locate a <see cref="ObjectFormatter"/> for a given type.
		/// 
		/// Using parameter, it's possible to extend an existing collection on an ad-hoc basis without modifying it.  Typically, for example, a collection
		/// might be initialised with the <see cref="Default"/> collection passed as the argument to this parameter, thus allowing a component to use the 
		/// default collection of formatters, but overriding and extending them as required without replacing any other formatters that are actually required
		/// by other components.</param>
		public ObjectFormatterCollection(ObjectFormatterCollection innerCollection = null)
		{
			_formatters = new ConcurrentDictionary<Type, ObjectFormatter>();
			//have to make sure all the inner collections are unique
			if (innerCollection != null)
			{
				//we can copy the reference across straight away, if an exception is thrown below, then the 
				//construction fails and it's not used anyway.
				_innerCollection = innerCollection;
				List<ObjectFormatterCollection> nestedCollections = new List<ObjectFormatterCollection>();

				while (innerCollection != null)
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
		public void AddFormatter<TObject>(ObjectFormatter formatter)
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

		public void AddFormatter<TObject>(ObjectFormatter<TObject> formatter)
		{
			AddFormatter(typeof(TObject), formatter);
		}

		public void AddFormatter(Type objectType, ObjectFormatter formatter)
		{
			objectType.MustNotBeNull(nameof(objectType));
			formatter.MustNotBeNull(nameof(formatter));
			objectType.MustNot(t => t == typeof(IFormattable), "Cannot explicitly add a formatter for the IFormattable type");

			_formatters[objectType] = formatter;
		}

		/// <summary>
		/// Produces a string representation of the arguments provided using this object (and the
		/// <see cref="ObjectFormatter"/> instances that have been registered within it) as a custom formatter.
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
		/// type, leveraging the <see cref="ObjectFormatter"/> objects contained within this collection
		/// to produce the string.
		/// </summary>
		/// <param name="format">The formattable object.</param>
		/// <remarks>This function is useful when you want to format a log message from a .Net interpolated
		/// string using the <see cref="ObjectFormatter"/> objects that have been registered in this collection.
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
		/// Formats the given object using a <see cref="ObjectFormatter" /> registered in this collection by a type which is
		/// equal to, an interface or base of, or an open generic of the object's type.
		/// 
		/// If no <see cref="ObjectFormatter"/> has been registered with a compatible type, but object implements the <see cref="IFormattable"/> interface,
		/// then its implementation of the <see cref="IFormattable.ToString(string, IFormatProvider)"/> will be used, optionally passing through the 
		/// <paramref name="format"/> and <paramref name="formatProvider"/> arguments if provided.
		/// 
		/// Otherwise, the <see cref="object.ToString"/> function is called.
		/// </summary>
		/// <param name="obj">The object to be formatted.</param>
		/// <param name="format">Optional format string that can be used to customise how the object is output to the string.
		/// Either be passed to the handling formatter's <see cref="ObjectFormatter.Format(object, string)"/> function, 
		/// or the <see cref="IFormattable.ToString(string, IFormatProvider)"/> function if the object is <see cref="IFormattable"/>.</param>
		/// <param name="formatProvider">Optional. Only used if no compatible <see cref="ObjectFormatter"/> is registered in this collection
		/// and the object implements the <see cref="IFormattable"/> interface.  In this case, this argument will be passed through to the
		/// object's implementation of the <see cref="IFormattable.ToString(string, IFormatProvider)"/> function.</param>
		public string Format(object obj, string format = null, IFormatProvider formatProvider = null)
		{
			if (obj == null)
				return "[null]";

			string toReturn;
			foreach (var formatter in GetFormatters(obj))
			{
				toReturn = formatter.Format(obj, format, this);
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
		private IEnumerable<ObjectFormatter> GetFormatters(object obj)
		{
			if (obj != null)
			{
				ObjectFormatter formatter;
				foreach (var type in GetTypeSearchList(obj))
				{
					if ((formatter = GetFormatterForTypeExact(type)) != null)
						yield return formatter;
				}
			}
		}

		private ObjectFormatter GetFormatterForTypeExact(Type type)
		{
			//exact type match can come from this collection or from the inner collection (or from its inner collection, ad nauseam)
			ObjectFormatter toReturn;
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

			var baseType = TypeHelpers.BaseType(t);
			if (baseType != null)
			{
				foreach (var tt in ExplodeType(baseType))
				{
					yield return tt;
				}
			}
		}

		#region ICustomFormatter and IFormatProvider implementations
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
		#endregion

		public void AddFormattersFromAssembly(Assembly assembly)
		{
			assembly.MustNotBeNull(nameof(assembly));
			IEnumerable<TypeInfo> typesFiltered = null;
			if (assembly == TypeHelpers.GetAssembly(typeof(ObjectFormatterCollection)))
				typesFiltered = assembly.DefinedTypes.Where(ti => !ti.IsAbstract && !ti.IsGenericTypeDefinition && (ti.IsPublic || ti.IsNotPublic)); //public and internal types
			else
				typesFiltered = assembly.DefinedTypes.Where(ti => !ti.IsAbstract && !ti.IsGenericTypeDefinition && ti.IsPublic);

			Dictionary<Type, List<TypeInfo>> registrations = new Dictionary<Type, List<TypeInfo>>();

			foreach (var result in typesFiltered.Select(ti => new { Type = ti, Attribute = ti.GetCustomAttribute<ObjectFormatterAttribute>() }).Where(t => t.Attribute != null))
			{
				if (typeof(ObjectFormatter).GetTypeInfo().IsAssignableFrom(result.Type))
				{
					Type[] associatedTypes = GetAssociatedTypesForFormatterType(assembly, result.Type, result.Attribute);

					if (associatedTypes.Length == 0)
					{
						throw new InvalidOperationException(string.Format("Cannot determine the type that the type {0} defined in {1} is responsible for formatting.  The ObjectFormatterAttribute is present on this type, but no associated types have been set.  Please add at least one associated type, or derive the formatter from ObjectFormatter<[target type]>", result.Type.FullName, assembly.FullName));
					}

					foreach (var type in associatedTypes.Distinct())
					{
						List<TypeInfo> list = null;
						if (!registrations.TryGetValue(type, out list))
							registrations[type] = list = new List<TypeInfo>();
						list.Add(result.Type);
					}
				}
				else
					throw new InvalidOperationException(string.Format("The Type \"{0}\" defined in {1} has the ObjectFormatterAttribute but does not inherit from ObjectFormatter", result.Type.FullName, assembly.FullName));
			}
			if (registrations.Count != 0)
			{
				var dupes = registrations.Where(kvp => kvp.Value.Count > 1).ToArray();
				if (dupes.Length != 0)
				{
					throw new InvalidOperationException(string.Format("One or more types tagged with the ObjectFormatterAttribute in the assembly {0} are associated to the same type: {1}",
						assembly.FullName,
						string.Join("; ", dupes.Select(kvp => string.Format("Target: {0}, Formatters: {1}", kvp.Key.FullName, string.Join(", ", kvp.Value.Select(t => t.FullName)))))));
				}
				Func<ObjectFormatterCollection, ObjectFormatter> factory = null;
				foreach (var registration in registrations)
				{
					try
					{
						factory = GetObjectFormatterFactory(registration.Value[0]);
					}
					catch (ArgumentException argEx)
					{
						throw new InvalidOperationException($"An instance of { registration.Value[0].FullName } cannot be registered for the type { registration.Key.FullName } because there is no way to construct an instance, see the inner exception for more", argEx);
					}

					try
					{
						AddFormatter(registration.Key, factory(this));
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException($"Constructing an instance of { registration.Value[0].FullName } for the type { registration.Key.FullName } failed with an exception, therefore registration has failed.  See the inner exception for more", ex);
					}
				}
			}
		}

		private static Type[] GetAssociatedTypesForFormatterType(Assembly assembly, TypeInfo tempType, ObjectFormatterAttribute tempTypeAttribute)
		{
			Type[] associatedTypes = tempTypeAttribute.AssociatedTypes;
			// usually, the attribute will be constructed with at least one type that the formatter should be associated with,
			// however, this is not required if the ObjectFormatter<T> class is in the inheritance chain, because the <T> is the 
			// default type.
			// note that we still allow a type inheriting from ObjectFormatter<T> to have its associated types set explicitly,
			// which will override this auto-detection.
			if (associatedTypes.Length == 0)
			{
				//now have to figure out whether the type is ObjectFormatter<T>.  If so, then its associated type is the type which is passed as the 
				//argument to that generic Type.
				foreach (var t in tempType.GetAllBases())
				{
					if (TypeHelpers.IsGenericType(t) && t.GetGenericTypeDefinition() == typeof(ObjectFormatter<>))
					{
						//get the type argument that was passed for <T> and use that, unless it's open
						var candidateType = TypeHelpers.GetGenericArguments(t)[0];
						if (!candidateType.IsGenericParameter)
							associatedTypes = new[] { candidateType };
						else
							throw new InvalidOperationException($"The type \"{ tempType.FullName }\" defined in { assembly.FullName } should not be decorated with the ObjectFormatterAttribute as it is a generic type definition.  Only types derived from a closed ObjectFormatter<T> can be decorated with the ObjectFormatterAttribute");
					}
				}
			}

			return associatedTypes;
		}

		private Func<ObjectFormatterCollection, ObjectFormatter> GetObjectFormatterFactory(TypeInfo objectFormatterType)
		{
			//look for a constructor which accepts a ObjectFormatterCollection instance
			ConstructorInfo ctor = objectFormatterType.DeclaredConstructors.SingleOrDefault(c =>
			{
				var parms = c.GetParameters();
				return parms.Length == 1 && parms[0].ParameterType == typeof(ObjectFormatterCollection);
			});

			ParameterExpression collExpr = Expression.Parameter(typeof(ObjectFormatterCollection), "collection");

			if (ctor != null)
			{
				return Expression.Lambda<Func<ObjectFormatterCollection, ObjectFormatter>>(Expression.Convert(Expression.New(ctor, collExpr), typeof(ObjectFormatter)), collExpr).Compile();
			}
			else if ((ctor = objectFormatterType.DeclaredConstructors.SingleOrDefault(c => c.GetParameters().Length == 0)) != null)
			{
				return Expression.Lambda<Func<ObjectFormatterCollection, ObjectFormatter>>(Expression.Convert(Expression.New(ctor), typeof(ObjectFormatter)), collExpr).Compile();
			}

			throw new ArgumentException($"Unable to find a default constructor or constructor with one parameter of type ObjectFormatterCollection on the type {objectFormatterType.FullName}", nameof(objectFormatterType));
		}

	}
}
