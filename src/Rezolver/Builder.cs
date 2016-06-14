using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
	/// <summary>
	/// Stores and retrieves registrations of <see cref="ITarget"/>s, is also Generic type aware,
	/// unlike its base class - <see cref="TargetDictionaryContainer"/>.
	/// 
	/// Should be used as the root target container for implementations of <see cref="IContainer"/>.
	/// </summary>
	/// <remarks>This is the type used by default for the <see cref="IContainer.Builder"/> of the 
	/// <see cref="Container"/> and <see cref="ScopedContainer"/> when you don't supply an instance of an 
	/// <see cref="ITargetContainer"/> explicitly on construction.</remarks>
	public class Builder : TargetDictionaryContainer
	{
		/// <summary>
		/// Constructs a new instance of the <see cref="Builder"/> class
		/// </summary>
		/// <param name="autoRezolveIEnumerable">If true, then <see cref="IEnumerable{T}"/> will be automatically
		/// resolved as a concatenation of all the <see cref="ITarget"/>s that are registered against a particular type.
		/// 
		/// Note - this parameter might be removed in a future version - you can achieve the same thing by using the
		/// extension method <see cref="EnumerableTargetBuilderExtensions.EnableEnumerableResolving(Builder)"/></param>
		public Builder(bool autoRezolveIEnumerable = true)
		{
			//TODO: Change this 
			if (autoRezolveIEnumerable)
			{
				this.EnableEnumerableResolving();
			}
		}

		/// <summary>
		/// Called to create and register a container for the given <paramref name="serviceType"/> and
		/// <paramref name="target"/>.
		/// 
		/// This class overrides the base version (<see cref="TargetDictionaryContainer.CreateContainer(Type, ITarget)"/>)
		/// to create a specialised container for generic types (<see cref="GenericTargetContainer"/>) if <paramref name="serviceType"/>
		/// if a generic type or generic type definition.
		/// </summary>
		/// <param name="serviceType">The type for which a container is to be created and registered.</param>
		/// <param name="target">Optional.  The target that will be added to the container that is returned.</param>
		/// <returns>An <see cref="ITargetContainer"/> in which the passed <paramref name="target"/> will
		/// be registered.</returns>
		/// <remarks>
		/// The main caller for this method will be the base Register method, which will create a 
		/// new container for a target that's being registered against a new type.
		/// 
		/// It is, however, also called by this class' implementation of <see cref="RegisterContainer(Type, ITargetContainer)"/>
		/// when the type is a generic type - as all generics must have a container registered against their generic type
		/// definitions as a starting point.</remarks>
		protected override ITargetContainer CreateContainer(Type serviceType, ITarget target)
		{
			if (TypeHelpers.IsGenericType(serviceType))
			{
				if(!TypeHelpers.IsGenericTypeDefinition(serviceType))
					serviceType = serviceType.GetGenericTypeDefinition();

				var created = CreateGenericTypeDefContainer(serviceType, target);
				//bypass the generic type detection in our override of RegisterContainer.
				RegisterContainerDirect(serviceType, created);
				return created;
			}
			else
				return base.CreateContainer(serviceType, target);
		}

		/// <summary>
		/// Called by <see cref="CreateContainer(Type,ITarget)"/> to create a container suitable for handling targets 
		/// that are registered against generic types.
		/// </summary>
		/// <param name="genericTypeDefinition">Will be an open generic type (generic type definition)</param>
		/// <param name="target">Optional.  The initial target for which the container is being constructed</param>
		/// <returns>The base implementation always creates an instance of <see cref="CreateGenericTypeDefContainer( Type,ITarget)"/></returns>
		protected virtual ITargetContainer CreateGenericTypeDefContainer(Type genericTypeDefinition, ITarget target)
		{
			return new GenericTargetContainer(genericTypeDefinition);
		}

		/// <summary>
		/// Retrieves 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public override ITargetContainer FetchContainer(Type type)
		{
			if (TypeHelpers.IsGenericType(type) && !TypeHelpers.IsGenericTypeDefinition(type))
				return base.FetchContainer(type.GetGenericTypeDefinition());

			return base.FetchContainer(type);
		}

		public override void Register(ITarget target, Type serviceType = null)
		{
			target.MustNotBeNull(nameof(target));
			if (serviceType != null && !target.SupportsType(serviceType))
				throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, serviceType), nameof(target));

			base.Register(target, serviceType);
		}
		/// <summary>
		/// Overrides the base method so that if <paramref name="type"/> is a generic type,
		/// then the container will be registered inside another which will be registered
		/// for the generic type definition first.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="container"></param>
		public override void RegisterContainer(Type type, ITargetContainer container)
		{
			//containers registered under generic types must always start with a generic target
			//container if one isn't already registered.
			if (TypeHelpers.IsGenericType(type))
			{
				//if it's not a generic type definition,
				//we need to ensure that the 
				if (!TypeHelpers.IsGenericTypeDefinition(type))
				{
					//make sure we definitely have a container for the generic type definition
					ITargetContainer genericTypeDefContainer = EnsureContainer(type.GetGenericTypeDefinition());
					//if the container is an ITargetContainerOwner, then we pass the call on to that
					//if it's not, then we throw an exception because there's no valid way for us to 
					//register this container.
					ITargetContainerOwner genericTypeDefContainerOwner = genericTypeDefContainer as ITargetContainerOwner;
					if (genericTypeDefContainerOwner != null)
						genericTypeDefContainerOwner.RegisterContainer(type, container);
					else
						throw new InvalidOperationException($"Container { genericTypeDefContainer } registered for the generic type definition { type } is not an ITargetContainerOwner - cannot register this container");
					return;
				}
			}

			RegisterContainerDirect(type, container);
		}

		private ITargetContainer EnsureContainer(Type type)
		{
			return FetchContainer(type) ?? CreateContainer(type, null);
		}

		/// <summary>
		/// Version of <see cref="RegisterContainer(Type, ITargetContainer)"/> which does not interrogate the 
		/// <paramref name="type"/> to see if it's generic - simply registers the passed container directly
		/// against the passed type (it just chains through directly to the 
		/// <see cref="TargetDictionaryContainer.RegisterContainer(Type, ITargetContainer)"/> method non-virtually.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="container"></param>
		protected virtual void RegisterContainerDirect(Type type, ITargetContainer container)
		{
			base.RegisterContainer(type, container);
		}
	}
}
