using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// An <see cref="ITargetContainerOwner"/> implementation that stores and retrieves 
	/// <see cref="ITarget"/> and <see cref="ITargetContainer"/> by type.
	/// </summary>
	/// <remarks>
	/// This type is not thread-safe
	/// 
	/// Note that for generic type, a special container is registered first against the 
	/// open generic version of the type, with concrete (closed) generics being registered within
	/// that.
	/// </remarks>
	public class TargetDictionaryContainer : ITargetContainer, ITargetContainerOwner
	{
		private readonly Dictionary<Type, ITargetContainer> _targets
			= new Dictionary<Type, ITargetContainer>();

		public IEnumerable<Type> AllRegisteredTypes { get { return _targets.Keys; } }
		public virtual ITarget Fetch(Type type)
		{
			type.MustNotBeNull(nameof(type));
			var container = FetchContainer(type);
			if (container == null) return null;
			return container.Fetch(type);
		}

		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			type.MustNotBeNull(nameof(type));
			var container = FetchContainer(type);
			if (container == null) return Enumerable.Empty<ITarget>();
			return container.FetchAll(type);
		}

		/// <summary>
		/// Obtains a child container that was previously registered by the passed
		/// <paramref name="type"/>.
		/// 
		/// Returns null if no entry is found.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual ITargetContainer FetchContainer(Type type)
		{
			type.MustNotBeNull(nameof(type));
			ITargetContainer toReturn;
			_targets.TryGetValue(type, out toReturn);
			return toReturn;
		}

		public virtual void RegisterContainer(Type type, ITargetContainer container)
		{
			type.MustNotBeNull(nameof(type));
			container.MustNotBeNull(nameof(container));

			ITargetContainer existing;
			_targets.TryGetValue(type, out existing);
			//if there is already another container registered, we attempt to combine the two, prioritising
			//the new container over the old one but trying the reverse operation if that fails.
			if (existing != null)
			{
				//ask the 'new' one how it wishes to be combined with the other or, if that doesn't support
				//combining, then try the existing container and see if that can.
				//If neither can (NotSupportedException is expected here) then this 
				//operation fails.
				try
				{
					_targets[type] = container.CombineWith(existing, type);
				}
				catch (NotSupportedException)
				{
					try
					{
						_targets[type] = existing.CombineWith(container, type);
					}
					catch (NotSupportedException)
					{
						throw new ArgumentException($"Cannot register the container because a container has already been registered for the type { type } and neither container supports the CombineWith operation");
					}
				}
			}
			else //no existing container - simply add it in.
				_targets[type] = container;
		}

		public virtual void Register(ITarget target, Type serviceType = null)
		{
			target.MustNotBeNull(nameof(target));
			serviceType = serviceType ?? target.DeclaredType;

			ITargetContainer container = FetchContainer(serviceType);
			if (container == null)
				container = CreateContainer(serviceType, target);

			container.Register(target, serviceType);
		}

		/// <summary>
		/// Called by <see cref="Register(ITarget, Type)"/> to create and register the container 
		/// instance most suited for the passed target.  For most types, the base implementation 
		/// will create a <see cref="TargetListContainer"/>.
		/// 
		/// For generic types, it call <see cref="CreateGenericTypeDefContainer( Type,ITarget)"/>, 
		/// passing the generic type definition (open generic) of that generic instead.
		/// <param name="serviceType"></param>
		/// <param name="target">The initial target for which the container is being created.
		/// Can be null.  Note - the function is not expected to add this target to the new
		/// container.</param>
		/// <returns></returns>
		protected virtual ITargetContainer CreateContainer(Type serviceType, ITarget target)
		{
			var created = new TargetListContainer(serviceType);

			RegisterContainer(serviceType, created);
			return created;
		}

	

		/// <summary>
		/// Always adds this container into the <paramref name="existing"/> container as a child.
		/// </summary>
		/// <param name="existing"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}
	}
}
