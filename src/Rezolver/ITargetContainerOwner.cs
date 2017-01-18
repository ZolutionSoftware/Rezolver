// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{
	/// <summary>
	/// Interface for an <see cref="ITargetContainer"/> which also contains other target containers.
	/// 
	/// It is not typically used by your application code since it's primarily an infrastructure interface - if you are extending the
	/// API, however, then you might need to work with it.
	/// </summary>
	/// <remarks>This interface, its implementations and everything else associated with it, is at the heart of functionality such as 
	/// open generics, automatic enumerables and decorators.</remarks>
	public interface ITargetContainerOwner : ITargetContainer
	{
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