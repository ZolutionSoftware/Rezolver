﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Standard implementation of <see cref="ObjectTargetMetadataBase"/>, to encapsulate object references
	/// that are to be baked into a rezolver as targets.
	/// </summary>
	/// <remarks>
	/// This class accepts either an object reference 
	/// that is to be returned when the target is resolved; or a delegate that will be called with the desired
	/// type (passed in from <see cref="GetObject"/>).
	/// </remarks>
	public class ObjectTargetMetadata : ObjectTargetMetadataBase, IObjectTargetMetadata
	{
		private readonly Func<Type, object> _valueProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectTargetMetadata"/> class.
		/// </summary>
		/// <param name="obj">The object that is to be returned from <see cref="GetObject"/>.</param>
		public ObjectTargetMetadata(object obj)
		{
			_valueProvider = (t) => obj;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectTargetMetadata"/> class.
		/// </summary>
		/// <param name="valueProvider">The value provider that will be called.</param>
		public ObjectTargetMetadata(Func<Type, object> valueProvider)
		{
			_valueProvider = valueProvider;
		}

		/// <summary>
		/// Called to get the object that will be registered in the IRezolverBuilder to be returned when a
		/// caller requests one of its registered types. The method can construct an object anew everytime it is
		/// called, or it can always return the same instance; this behaviour is implementation-dependant.
		/// </summary>
		/// <param name="type">The type of object that is desired.  The implementation determines whether this
		/// parameter is required.  If it is, and you pass null, then an ArgumentNullException will be thrown.
		/// If you pass an argument, the implementation is not bound to check or honour the type.  Its purpose
		/// is to provide a hint only, not a guarantee that the object returned is compatible with the type.</param>
		/// <returns>An object.  Note - if the operation returns null this is not an error.</returns>
		/// <exception cref="System.ArgumentNullException">type</exception>
		public override object GetObject(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			//note - no guarantee is made that the returned object is actually compatible
			//with the given type.
			return _valueProvider(type);
		}
	}
}
