// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Options;
using Rezolver.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="ITargetContainer"/> implementation that stores and retrieves 
    /// <see cref="ITarget"/> and <see cref="ITargetContainer"/> by service type.
    /// </summary>
    /// <remarks>
    /// This class stores <see cref="ITarget"/> instances inside child <see cref="ITargetContainer"/> instances that are registered
    /// against types equal, or related, to the <see cref="ITarget.DeclaredType"/> of the target.
    /// 
    /// When <see cref="Register(ITarget, Type)"/> is called, an <see cref="ITargetContainer"/> is looked up which will 'own' that target.
    /// If one is not found, then one will be created and automatically registered (via <see cref="EnsureContainer(Type)"/>.
    /// 
    /// With a (possibly new) child target container in hand, the registration is then delegated to that target container.
    /// </remarks>
    public class TargetDictionaryContainer : ITargetContainer
    {
        private readonly Dictionary<Type, ITargetContainer> _targetContainers
          = new Dictionary<Type, ITargetContainer>();

        /// <summary>
        /// Never null.  Returns the root target container.
        /// </summary>
        /// <value>If this instance is created with a root
        /// passed to the <see cref="TargetDictionaryContainer(ITargetContainer)"/>
        /// constructor, then it will be returned by this property.
        /// 
        /// Otherwise it will return this instance.</value>
        protected ITargetContainer Root { get; }

        /// <summary>
        /// Constructs a new <see cref="TargetDictionaryContainer"/> optionally setting the <see cref="Root"/> target container.
        /// </summary>
        /// <param name="root">If this container belongs to one overarching root container (typically an instance of 
        /// <see cref="TargetContainer"/> or <see cref="OverridingTargetContainer"/>), then pass it here.</param>
        /// <remarks>The importance of the <paramref name="root"/> target container is to enable code to be able to reach all 
        /// registrations for all services rather than only those which are stored within this container, because this type is used
        /// both as a root, but also for other more specialised target containers.</remarks>
        public TargetDictionaryContainer(ITargetContainer root = null)
        {
            Root = root ?? this;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Fetch(Type)"/>.
        /// </summary>
        /// <param name="type">The type whose default target is to be retrieved.</param>
        /// <returns>A single target representing the last target registered against the 
        /// <paramref name="type"/>, or, null if no target is found.</returns>
        /// <remarks>Note - in scenarios where you are chaining multiple containers, then
        /// you should consult the return value's <see cref="ITarget.UseFallback"/> property
        /// if the method returns non-null because, if true, then it's an instruction to
        /// use a parent container's result for the same type.</remarks>
        public virtual ITarget Fetch(Type type)
        {
            type.MustNotBeNull(nameof(type));
            var container = FetchContainer(type);
            if (container == null) return null;
            return container.Fetch(type);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.FetchAll(Type)"/>
        /// </summary>
        /// <param name="type">The type whose targets are to be retrieved.</param>
        /// <returns>A non-null enumerable containing the targets that match the type, or an
        /// empty enumerable if the type is not registered.</returns>
        public virtual IEnumerable<ITarget> FetchAll(Type type)
        {
            type.MustNotBeNull(nameof(type));
            var container = FetchContainer(type);
            if (container == null) return Enumerable.Empty<ITarget>();
            return container.FetchAll(type);
        }

        /// <summary>
        /// Obtains the child container which owns the given <paramref name="serviceType"/> on behalf
        /// of this target container.  
        ///
        /// Returns null if no entry is found.
        /// </summary>
        /// <param name="serviceType">The service type whose owning <see cref="ITargetContainer"/> is sought.</param>
        /// <returns>The target container which manages the given service type, if one is registered - otherwise <c>null</c>.</returns>
        public virtual ITargetContainer FetchContainer(Type serviceType)
        {
            serviceType.MustNotBeNull(nameof(serviceType));
            _targetContainers.TryGetValue(GetTargetContainerType(serviceType), out ITargetContainer toReturn);
            return toReturn;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)" />
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        /// <remarks>This container implementation actually stores containers against the types that targets are registered
        /// against, rather than simply storing a dictionary of targets.  This method allows you to add your own containers
        /// against type (instead of the default, which is <see cref="TargetListContainer"/>) so you can plug in some advanced
        /// behaviour into this container.
        /// 
        /// For example, decorators are not actually <see cref="ITarget"/> implementations but specialised <see cref="ITargetContainer"/>
        /// instances into which the 'standard' targets are registered.</remarks>
        public virtual void RegisterContainer(Type type, ITargetContainer container)
        {
            type.MustNotBeNull(nameof(type));
            container.MustNotBeNull(nameof(container));
            _targetContainers.TryGetValue(type, out ITargetContainer existing);
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
                    _targetContainers[type] = container.CombineWith(existing, type);
                }
                catch (NotSupportedException)
                {
                    try
                    {
                        _targetContainers[type] = existing.CombineWith(container, type);
                    }
                    catch (NotSupportedException)
                    {
                        throw new ArgumentException($"Cannot register the container because a container has already been registered for the type { type } and neither container supports the CombineWith operation");
                    }
                }
            }
            else //no existing container - simply add it in.
                _targetContainers[type] = container;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/>.
        /// </summary>
        /// <param name="target">The target to be registered</param>
        /// <param name="serviceType">The service type against which the <paramref name="target"/> is to be registered, if 
        /// different from the target's <see cref="ITarget.DeclaredType"/>.</param>
        /// <remarks>This implementation creates a child <see cref="ITargetContainer"/> for the <paramref name="serviceType"/> 
        /// with a call to the protected method <see cref="EnsureContainer(Type)"/> if one doesn't already exist.
        /// 
        /// The registration is then delegated to that child container's own implementation of
        /// <see cref="ITargetContainer.Register(ITarget, Type)"/>.</remarks>
        public virtual void Register(ITarget target, Type serviceType = null)
        {
            target.MustNotBeNull(nameof(target));
            serviceType = serviceType ?? target.DeclaredType;

            ITargetContainer container = EnsureContainer(serviceType);

            container.Register(target, serviceType);
        }

        /// <summary>
        /// Called to make sure that an <see cref="ITargetContainer"/> has been registered which can act as
        /// owner to targets whose <see cref="ITarget.DeclaredType"/> is equal to the passed <paramref name="serviceType"/> 
        /// so that a target or target container can be registered.
        /// </summary>
        /// <param name="serviceType">Required. The service type against which a target registration is to be made (and for
        /// which a target container is required.</param>
        /// <returns>A child target container into which targets, or another child target container, can be registered.</returns>
        /// <remarks>
        /// An existing container is returned if <see cref="FetchContainer(Type)"/> returns it.  If not, then a new container 
        /// is created and registered via the <see cref="AutoRegisterContainer(Type)"/> method, and returned.
        /// 
        /// Note that the <paramref name="serviceType"/> will possibly be mapped to a different container type via any
        /// <see cref="ITargetContainerTypeResolver"/> that's registered via the options API.</remarks>
        protected ITargetContainer EnsureContainer(Type serviceType)
        {
            return FetchContainer(serviceType) ?? AutoRegisterContainer(GetTargetContainerType(serviceType));
        }

        /// <summary>
        /// Called by <see cref="Register(ITarget, Type)"/> to create and register the container 
        /// instance most suited for the passed target.  The base implementation 
        /// always creates a <see cref="TargetListContainer"/>, capable of storing multiple targets
        /// against a single type.</summary>
        /// <param name="targetContainerType">The type that the target container is to be registered under.
        /// 
        /// Note that this type will *not* be remapped by the <see cref="GetTargetContainerType(Type)"/> method - 
        /// it will be used as-is.</param>
        /// <returns></returns>
        protected virtual ITargetContainer AutoRegisterContainer(Type targetContainerType)
        {
            var created = CreateContainer(targetContainerType);
            RegisterContainer(targetContainerType, created);
            return created;
        }

        /// <summary>
        /// Called to get the type that's to be used to fetch a child <see cref="ITargetContainer"/> for targets registered
        /// against a given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type - usually pulled from the <see cref="ITarget.DeclaredType"/> of a 
        /// <see cref="ITarget"/> that is to be registered, or the service type passed to <see cref="Fetch(Type)"/>.</param>
        /// <returns>The base implementation always returns the <paramref name="serviceType"/></returns>
        protected virtual Type GetTargetContainerType(Type serviceType)
        {
            return serviceType;
        }

        /// <summary>
        /// Called to create a new container instance which will be 
        /// </summary>
        /// <param name="targetContainerType"></param>
        /// <returns></returns>
        protected virtual ITargetContainer CreateContainer(Type targetContainerType)
        {
            return new TargetListContainer(Root, targetContainerType);
        }

        /// <summary>
        /// Not supported by default
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
