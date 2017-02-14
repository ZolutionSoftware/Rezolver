// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Extensions for <see cref="ITargetContainer"/> relating to registering concrete instances via the <see cref="ObjectTarget"/>
	/// target.
	/// </summary>
	public static class ObjectTargetContainerExtensions
	{
		/// <summary>
		/// Registers an instance to be used when resolving a particular service type via the <see cref="ObjectTarget"/>
		/// target.
		/// </summary>
		/// <typeparam name="T">Type of the object - will be used as the service type for registration if
		/// <paramref name="serviceType"/> is not provied.</typeparam>
		/// <param name="targetContainer">The target container which will receive the registration.</param>
		/// <param name="obj">The instance that will be resolved when the service type is requested.</param>
		/// <param name="serviceType">The service type against which this object is to be registered, if different
		/// from <typeparamref name="T"/>.</param>
		/// <param name="scopeBehaviour">Sets the <see cref="ITarget.ScopeBehaviour"/> for the 
		/// <see cref="ObjectTarget"/> that's created</param>
		public static void RegisterObject<T>(this ITargetContainer targetContainer, T obj, Type serviceType = null, ScopeBehaviour scopeBehaviour = ScopeBehaviour.None)
		{
			targetContainer.MustNotBeNull(nameof(targetContainer));
			targetContainer.Register(obj.AsObjectTarget(serviceType, scopeBehaviour: scopeBehaviour), serviceType);
		}
	}
}
