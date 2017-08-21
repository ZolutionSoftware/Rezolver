// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// An interface used by <see cref="TargetDictionaryContainer"/> (via the options API)
    /// to identify the container type for targets with a specific <see cref="ITarget.DeclaredType"/>.
    /// </summary>
    public interface ITargetContainerTypeResolver
    {
        /// <summary>
        /// For operations such as <see cref="TargetDictionaryContainer.Fetch(Type)"/> and 
        /// <see cref="TargetDictionaryContainer.FetchContainer(Type)"/>, the type requested might sometimes
        /// need to be redirected to another for the purposes of fetching the correct <see cref="ITargetContainer"/>
        /// for a given type of service.  If this method returns a non-null type, then the calling container
        /// will use that type instead of the original one passed when trying to locate a container for the
        /// service type.
        /// </summary>
        /// <param name="serviceType">The service type (equal to the <see cref="ITarget.DeclaredType"/> of 
        /// any <see cref="ITarget"/> objects that might have been registered).</param>
        /// <returns>A type that should be used to look up a container, if different from the <paramref name="serviceType"/>,
        /// otherwise <c>null</c>.</returns>
        Type GetContainerType(Type serviceType);
    }

    public interface ITargetContainerFactory
    {
        ITargetContainer CreateContainer(Type serviceType, ITarget target, ITargetContainer targets, ITargetContainer rootTargets);
    }

    /// <summary>
    /// An <see cref="ITargetContainer"/> implementation that stores and retrieves 
    /// <see cref="ITarget"/> and <see cref="ITargetContainer"/> by type.
    /// </summary>
    /// <remarks>
    /// This type is not thread-safe
    /// 
    /// Note that for generic type, a special container is registered first against the 
    /// open generic version of the type, with concrete (closed) generics being registered within
    /// that.
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
        /// Constructs a new <see cref="TargetDictionaryContainer"/> optionally setting 
        /// </summary>
        /// <param name="root">If this container belongs to another, then pass it here.</param>
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
        /// Obtains a child container that was previously registered by the passed
        /// <paramref name="type"/>.  Note - the type 
        /// 
        /// Returns null if no entry is found.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual ITargetContainer FetchContainer(Type type)
        {
            type.MustNotBeNull(nameof(type));
#error TODO: re-read the ITargetContainerTypeResolver option to see if the type needs redirecting.
            _targetContainers.TryGetValue(type, out ITargetContainer toReturn);
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
        /// <param name="serviceType"></param>
        /// <remarks>This implementation creates an <see cref="ITargetContainer"/> for the <paramref name="serviceType"/> 
        /// with a call to the protected method <see cref="AutoRegisterContainer(Type, ITarget)"/> if one doesn't exist 
        /// (it calls <see cref="FetchContainer(Type)"/> to check for existence),
        /// and then chains to its <see cref="ITargetContainer.Register(ITarget, Type)"/> method.</remarks>
        public virtual void Register(ITarget target, Type serviceType = null)
        {
            target.MustNotBeNull(nameof(target));
            serviceType = serviceType ?? target.DeclaredType;

            ITargetContainer container = EnsureContainer(serviceType, target);

            container.Register(target, serviceType);
        }

        /// <summary>
        /// Called to make sure that an <see cref="ITargetContainer"/> has been registered which can act as
        /// owner to the passed <paramref name="serviceType"/> so that a target or target container can be
        /// registered.
        /// 
        /// The base implementation tries to get an <see cref="ITargetContainerTypeResolver"/> via the options API
        /// (from itself) to see if a different container type should be used, or if the original service type should
        /// be used.
        /// </summary>
        /// <param name="serviceType">Required. The service type against which a target registration is to be made (and for
        /// which a target container is required.</param>
        /// <param name="target">Optional. The target that is to be registered, if relevant.</param>
        /// <returns>The returned container will either already have been registered in this container, or might have
        /// been created and registered anew via the method <see cref="AutoRegisterContainer(Type, ITarget)"/>.</returns>
        protected ITargetContainer EnsureContainer(Type serviceType, ITarget target = null)
        {
            var typeResolver = this.GetOption<ITargetContainerTypeResolver>(serviceType);
            var redirectedType = typeResolver?.GetContainerType(serviceType) ?? serviceType;
            return FetchContainer(redirectedType) ?? AutoRegisterContainer(redirectedType, target);
        }

        /// <summary>
        /// Called by <see cref="Register(ITarget, Type)"/> to create and register the container 
        /// instance most suited for the passed target.  The base implementation 
        /// always creates a <see cref="TargetListContainer"/>, capable of storing multiple targets
        /// against a single type.</summary>
        /// <param name="serviceType"></param>
        /// <param name="target">The initial target for which the container is being created.
        /// Can be null.  Note - the function is not expected to add this target to the new
        /// container.</param>
        /// <returns></returns>
        protected virtual ITargetContainer AutoRegisterContainer(Type serviceType, ITarget target)
        {
            var created = new TargetListContainer(Root, serviceType);

            RegisterContainer(serviceType, created);
            return created;
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
