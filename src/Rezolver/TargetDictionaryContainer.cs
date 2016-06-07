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
	/// </remarks>
	public class TargetDictionaryContainer : ITargetContainer, ITargetContainerOwner
	{
		private readonly Dictionary<Type, ITargetContainer> _targets = new Dictionary<Type, ITargetContainer>();

		public IEnumerable<Type> AllRegisteredTypes { get { return _targets.Keys; } }
		public virtual ITarget Fetch(Type type)
		{
			var container = FetchContainer(type);
			if (container == null) return null;
			return container.Fetch(type);
		}

		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			var container = FetchContainer(type);
			if (container == null) return Enumerable.Empty<ITarget>();
			return container.FetchAll(type);
		}

		/// <summary>
		/// Obtains a child container which was previously registered by the passed <paramref name="type"/>.
		/// 
		/// Returns null if no entry is found.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual ITargetContainer FetchContainer(Type type)
		{
			ITargetContainer toReturn;
			_targets.TryGetValue(type, out toReturn);
			return toReturn;
		}

		public virtual void RegisterContainer(Type type, ITargetContainer container)
		{
			ITargetContainer existing;
			_targets.TryGetValue(type, out existing);
			//if there is already another container registered, we have to decide which one becomes
			//registered and which one becomes the new child.
			if (existing != null)
			{
				var existingAsOwner = existing as ITargetContainerOwner;
				var containerAsOwner = container as ITargetContainerOwner;
				//decision is based on which of the two containers are container owners, i.e, capable
				//of storing other containers.
				//if the existing one is, but the new one isn't, then the new one gets added to the existing
				//if the existing one isn't, but the new one is, then the existing one gets added to the new one 
				//	and the new one replaces the existing one - using the Replace
				//if they both are, then we use the 
				if (containerAsOwner != null)
				{
					if (existingAsOwner != null)
					{
						//both are container owners, so ask the 'new' one how it wishes to be combined with the other.
						_targets[type] = containerAsOwner.CombineWith(existingAsOwner, type);
					}
					else
					{
						//new container replaces the old one, and the old one is registered to the new one
						//under the registration type
						containerAsOwner.RegisterContainer(type, existing);
						_targets[type] = container;
					}
				}
				else
				{
					if (existingAsOwner != null)
					{
						existingAsOwner.RegisterContainer(type, container);
					}
					else
						//this is only temporary I think - until we have a scenario where we do need to do this.
						throw new ArgumentException("A container is already registered for this type", nameof(type));
				}
			}
			else //no existing container - simply add it in.
				_targets[type] = container;
		}

		public virtual void Register(ITarget target, Type serviceType = null)
		{
			target.MustNotBeNull(nameof(target));
			serviceType = serviceType ?? target.DeclaredType;

			ITargetContainer container = null;

			_targets.TryGetValue(serviceType, out container);

			if (container != null)
				container.Register(target, serviceType);
			else
				_targets[serviceType] = CreateContainer(target, serviceType);
		}

		protected virtual ITargetContainer CreateContainer(ITarget target, Type serviceType)
		{
			return new TargetListContainer(serviceType, target);
		}


		/// <summary>
		/// Always adds this container into the <paramref name="existing"/> container as a child.
		/// </summary>
		/// <param name="existing"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual ITargetContainerOwner CombineWith(ITargetContainerOwner existing, Type type)
		{
			existing.RegisterContainer(type, this);
			return existing;
		}
	}
}
