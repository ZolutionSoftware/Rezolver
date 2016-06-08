using System;

namespace Rezolver
{
	/// <summary>
	/// Interface for an <see cref="ITargetContainer"/> which also contains other target containers.
	/// </summary>
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
		/// If a container already exists against this type, and the new <paramref name="container"/> is an <see cref="ITargetContainerOwner"/>,
		/// then the container that is returned by its <see cref="ChainTo(ITargetContainer,Type)"/> method will replace the existing one.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="container"></param>
		void RegisterContainer(Type type, ITargetContainer container);
	}
}