using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Stores and retrieves <see cref="ITarget"/> instances, keyed (primarily) by the type of service
	/// that the targets are associated with.
	/// </summary>
	public interface ITargetContainer
	{
		/// <summary>
		/// Registers a target, optionally for a particular target type.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="serviceType">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="ITarget.DeclaredType"/> of the <paramref name="target"/></param>
		void Register(ITarget target, Type serviceType = null);
		/// <summary>
		/// Searches for the target for a particular type
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <returns></returns>
		ITarget Fetch(Type type);
		IEnumerable<ITarget> FetchAll(Type type);	
	}
}
