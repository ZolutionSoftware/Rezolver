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
		/// <summary>
		/// Gets the container that represents the result of combining this container owner to an <paramref name="existing"/> container as part of a
		/// registration inside another <see cref="ITargetContainerOwner"/>.
		/// 
		/// Used most frequently in implementations of <see cref="RegisterContainer(Type, ITargetContainer)"/> when a container owner is already registered
		/// against the type, and a new container owner is then registered against the same type.
		/// </summary>
		/// <param name="existing">The existing <see cref="ITargetContainerOwner"/> instance that this instance is to be combined with</param>
		/// <param name="type">The type that the combined container owner will be registered under.</param>
		/// <returns></returns>
		/// <remarks>
		/// The two most obvious implementations of this could either:
		/// a) return the <paramref name="existing"/> container after potentially adding this container to it, to make this container a 'child' of the existing
		/// b) return this container after potentially adding the <paramref name="existing"/> container to it, to make this container the new owner of the existing
		/// 
		/// The second of these two behaviours is how the core API implements decorators.
		/// 
		/// Clearly, the implementation could also create a brand new container owner representing the combination of the two.
		/// </remarks>
		ITargetContainerOwner CombineWith(ITargetContainerOwner existing, Type type);
	}
}