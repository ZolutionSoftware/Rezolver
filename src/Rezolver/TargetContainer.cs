// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Root container for <see cref="ITarget"/>s that can be used as the backing for the standard
    /// <see cref="IContainer"/> classes - <see cref="Container"/> and <see cref="ScopedContainer"/>.
    /// 
    /// Stores and retrieves registrations of <see cref="ITarget"/>s, is also Generic type aware,
    /// unlike its base class - <see cref="TargetDictionaryContainer"/>.
    /// </summary>
    /// <remarks>This is the type used by default for the <see cref="ContainerBase.Targets"/> of all
    /// the standard containers in the core framework, e.g. <see cref="Container"/>, 
    /// <see cref="ScopedContainer"/> etc, when you don't supply an instance of an 
    /// <see cref="ITargetContainer"/> explicitly on construction.
    /// 
    /// Although you can derive from this class to extend its functionality; it's also possible to 
    /// extend it via configuration (see <see cref="ITargetContainerConfig"/>) - which is how, for example,
    /// the framework enables automatic injection of enumerables (see <see cref="Configuration.InjectEnumerables"/>) and
    /// lists (see <see cref="Configuration.InjectLists"/>).
    /// 
    /// The <see cref="DefaultConfig"/> is used for new instances which are not passed an explicit configuration.</remarks>
    public class TargetContainer : TargetDictionaryContainer
    {        
        /// <summary>
        /// The default configuration used for <see cref="TargetContainer"/> objects created via the <see cref="TargetContainer.TargetContainer(ITargetContainerConfig)"/>
        /// constructor when no configuration is explicitly passed.
        /// </summary>
        /// <remarks>The simplest way to configure all target container instances is to add/remove configs to this collection.
        /// 
        /// Note also that the <see cref="OverridingTargetContainer"/> class also uses this.
        /// 
        /// ## Default configurations
        /// 
        /// The configurations applied by default are:
        /// 
        /// - <see cref="Configuration.InjectEnumerables"/>
        /// - <see cref="Configuration.InjectLists"/>
        /// - <see cref="Configuration.InjectCollections"/>
        /// - <see cref="Configuration.InjectResolveContext"/>
        /// </remarks>
        public static CombinedTargetContainerConfig DefaultConfig { get; } = new CombinedTargetContainerConfig(new ITargetContainerConfig[]
        {
            Configuration.InjectEnumerables.Instance,
            Configuration.InjectLists.Instance,
            Configuration.InjectCollections.Instance,
            Configuration.InjectResolveContext.Instance
        });

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainer"/> class.
        /// </summary>
        /// <param name="config">Optional.  The configuration to apply to this target container.  If null, then
        /// the <see cref="DefaultConfig"/> is used.
        /// </param>
        /// <remarks>Note to inheritors: this constructor will throw an <see cref="InvalidOperationException"/> if called by derived
        /// classes.  You must instead use the <see cref="TargetContainer.TargetContainer()"/> constructor and apply configuration in your
        /// constructor.</remarks>
        public TargetContainer(ITargetContainerConfig config = null)
        {
            if (this.GetType() != typeof(TargetContainer))
                throw new InvalidOperationException("Derived types must not use this constructor because it triggers virtual method calls via the configuration callbacks.  Please use the protected parameterless constructor instead");

            (config ?? DefaultConfig).Configure(this);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TargetContainer"/> class without attaching any
        /// <see cref="ITargetContainerConfig"/> to it.  This is desirable for derived types as behaviours typically
        /// will invoke methods on this target container which are declared virtual and which are, therefore, 
        /// unsafe to be called during construction.
        /// </summary>
        protected TargetContainer()
        {

        }

        /// <summary>
        /// Called to create and register a container for the given <paramref name="serviceType"/> and
        /// <paramref name="target"/>.
        /// 
        /// This class overrides the base version (<see cref="TargetDictionaryContainer.AutoRegisterContainer(Type, ITarget)"/>)
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
        protected override ITargetContainer AutoRegisterContainer(Type serviceType, ITarget target)
        {
            if (TypeHelpers.IsGenericType(serviceType))
            {
                if (!TypeHelpers.IsGenericTypeDefinition(serviceType))
                    serviceType = serviceType.GetGenericTypeDefinition();
                //TODO: consider changing this functionality to use a factory registered within the target container itself.
                var created = CreateGenericTypeDefContainer(serviceType, target);
                RegisterContainer(serviceType, created);
                return created;
            }
            else
                return base.AutoRegisterContainer(serviceType, target);
        }

        /// <summary>
        /// Called by <see cref="AutoRegisterContainer(Type,ITarget)"/> to create a container suitable for handling targets 
        /// that are registered against generic types.
        /// </summary>
        /// <param name="genericTypeDefinition">Will be an open generic type (generic type definition)</param>
        /// <param name="target">Optional.  The initial target for which the container is being constructed</param>
        /// <returns>The base implementation always creates an instance of <see cref="CreateGenericTypeDefContainer( Type,ITarget)"/></returns>
        protected virtual ITargetContainer CreateGenericTypeDefContainer(Type genericTypeDefinition, ITarget target)
        {
            //Note below: Root will be equal to this
            return new GenericTargetContainer(Root, genericTypeDefinition);
        }

        /// <summary>
        /// Retrieves 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override ITargetContainer FetchContainer(Type type)
        {
            if (TypeHelpers.IsGenericType(type) && !TypeHelpers.IsGenericTypeDefinition(type))
                type = type.GetGenericTypeDefinition();

            return base.FetchContainer(type);
        }

        /// <summary>
        /// Overrides the base method to block registration if the <paramref name="target"/> does not support the 
        /// <paramref name="serviceType"/> (checked by calling the target's <see cref="ITarget.SupportsType(Type)"/> method).
        /// </summary>
        /// <param name="target">The target to be registered.</param>
        /// <param name="serviceType">Optional - the type against which the target is to be registered, if different from the
        /// target's <see cref="ITarget.DeclaredType"/>.</param>
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
                //we need to ensure that we have a container for the type's generic
                //type definition
                if (!TypeHelpers.IsGenericTypeDefinition(type))
                {
                    //make sure we definitely have a container for the generic type definition
                    ITargetContainer genericTypeDefContainer = EnsureContainer(type.GetGenericTypeDefinition());

                    genericTypeDefContainer.RegisterContainer(type, container);

                    return;
                }
            }
            //because the container is being registered directly against a generic type definition,
            //we register it directly.
            base.RegisterContainer(type, container);
        }
    }
}
