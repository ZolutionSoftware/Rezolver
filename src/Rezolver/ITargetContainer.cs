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

		/// <summary>
		/// If supported by the implementation, this gets the container that represents the result of combining this container to an <paramref name="existing"/> container as part of a
		/// registration inside another <see cref="ITargetContainerOwner"/>.
		/// 
		/// Used most frequently in implementations of <see cref="ITargetContainerOwner.RegisterContainer(Type, ITargetContainer)"/> when a container owner is already registered
		/// against the type, and a new container owner is then registered against the same type.
		/// </summary>
		/// <param name="existing">The existing <see cref="ITargetContainer"/> instance that this instance is to be combined with</param>
		/// <param name="type">The type that the combined container owner will be registered under.</param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">If this container doesn't support being combined with the other.
		/// </exception>
		ITargetContainer CombineWith(ITargetContainer existing, Type type);
	}
}
