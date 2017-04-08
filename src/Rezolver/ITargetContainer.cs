// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Stores and retrieves <see cref="ITarget"/> instances, keyed by the type of service
	/// that the targets are registered against.
	/// 
	/// This is where all your service registrations will ultimately go.
	/// 
	/// </summary>
	/// <remarks>You do not resolve objects from a target container, instead, it holds the <see cref="ITarget"/>s which will 
	/// later be compiled to produce the objects.
	/// 
	/// A target container is considered mutable for its entire lifetime, because it's only a glorified dictionary
	/// of targets from which multiple <see cref="IContainer"/> objects can be built (when using the types provided
	/// in the framework).
	/// 
	/// As an example, the <see cref="Container"/> class uses this as the source of the registrations that it uses to resolve objects
	/// in its <see cref="IContainer.Resolve(ResolveContext)"/> implementation.
	/// 
	/// Note that there are multiple implementations of this interface in the framework, however the two you will use most commonly
	/// are <see cref="TargetContainer"/> and <see cref="ChildTargetContainer"/>.</remarks>
	public interface ITargetContainer
	{
		/// <summary>
		/// Registers a target, either for the <paramref name="serviceType"/> specified or, if null, the <see cref="ITarget.DeclaredType"/>
		/// of the <paramref name="target"/>.
		/// </summary>
		/// <param name="target">Required.  The target to be registered</param>
		/// <param name="serviceType">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="ITarget.DeclaredType"/> of the <paramref name="target"/>.  If provided, then the <paramref name="target"/>
		/// must be compatible with this type.</param>
		/// <remarks>The interface does not define the behaviour for when multiple targets are registered against the same type - although
		/// the default behaviour of the framework (via the <see cref="TargetContainer"/> class) is to allow this, with the last registered target 
		/// being treated as the 'default' for that type.
		/// 
		/// The only guarantee expected by the framework from implementations of this interface is that if a target is registered against
		/// a given type with this method, then a <see cref="Fetch(Type)"/> operation with the same type should return a valid target,
		/// and a <see cref="FetchAll(Type)"/> operation should return a non-empty enumerable of targets.</remarks>
		/// <exception cref="System.ArgumentException">If <paramref name="serviceType"/> is non-null and the <paramref name="target"/>'s
		/// <see cref="ITarget.SupportsType(Type)"/> method returns <c>false</c> for that type.</exception>
		void Register(ITarget target, Type serviceType = null);

		/// <summary>
		/// Retrieves a target for a particular type, or null if no target is registered against that type.
		/// </summary>
		/// <param name="type">Required.  The type for which an <see cref="ITarget"/> is to be retrieved.</param>
		/// <returns>The target for the given <paramref name="type"/>, or null if no target is found.</returns>
		/// <remarks>The target you receive from this method depends entirely on the implementation.
		/// 
		/// It could be the last target to be registered against the <paramref name="type"/> or the first, or 
		/// another target entirely.
		/// 
		/// As mentioned in the documentation for the <see cref="Register(ITarget, Type)"/> method - the only guarantee is that if
		/// at least one target has been registered for the same type, then this method should return a valid target.</remarks>
		ITarget Fetch(Type type);

		/// <summary>
		/// Retrieves an enumerable of all targets that have been registered for a particular <paramref name="type"/>.  
		/// </summary>
		/// <param name="type">Required.  The type for which the <see cref="ITarget"/>s are to be retrieved.</param>
		/// <returns>An enumerable containing all the targets that have been registered against the given <paramref name="type"/>, or, 
		/// an empty enumerable if no targets have been registered.</returns>
		/// <remarks>As with <see cref="Fetch(Type)"/>, the only guarantee is that if a target has been registered for the <paramref name="type"/>
		/// through a call to <see cref="Register(ITarget, Type)"/>, then the returned enumerable will contain at least one valid target.</remarks>
		IEnumerable<ITarget> FetchAll(Type type);

		/// <summary>
		/// If supported by the implementation, this gets the container built from combining this container with 
		/// an <paramref name="existing"/> container as part of a registration inside another <see cref="ITargetContainer"/>.
		/// </summary>
		/// <param name="existing">The existing <see cref="ITargetContainer"/> instance that this instance is to be combined with</param>
		/// <param name="type">The type that the combined container owner will be registered under.</param>
		/// <returns></returns>
		/// <remarks>Used most frequently in implementations of <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> 
		/// when a container owner is already registered against the type, and a new container owner is then registered against the 
		/// same type.  This behaviour is used to implement open generics and decorators, and can be used to implement more besides.</remarks>
		/// <exception cref="System.NotSupportedException">If this container doesn't support being combined with another.</exception>
		ITargetContainer CombineWith(ITargetContainer existing, Type type);

        /// <summary>
		/// Retrieves an existing container registered against the given <paramref name="type"/>, or null if not found.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		ITargetContainer FetchContainer(Type type);
        /// <summary>
        /// Registers a container against a given <paramref name="type"/>.
        /// 
        /// If a container already exists against this type, then the existing container's 
        /// <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> method is called with the <paramref name="container"/> as the 
        /// argument, and the resulting container will replace the existing one.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        void RegisterContainer(Type type, ITargetContainer container);
    }
}
