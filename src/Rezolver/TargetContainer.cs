﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rezolver.Events;
using Rezolver.Runtime;

namespace Rezolver
{
    /// <summary>
    /// Root container for <see cref="ITarget"/>s that can be used as the backing for
    /// <see cref="Container"/> and <see cref="ScopedContainer"/>.
    ///
    /// Stores and retrieves registrations of <see cref="ITarget"/>s, is also Generic type aware,
    /// unlike its base class - <see cref="TargetDictionaryContainer"/>.
    /// </summary>
    /// <remarks>This is the type used by default for the <see cref="Container.Targets"/> of all
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
    public class TargetContainer : TargetDictionaryContainer, IRootTargetContainer
    {
        private static readonly ConcurrentDictionary<Type, ContainerTypeAttribute> _containerTypeLookup = new ConcurrentDictionary<Type, ContainerTypeAttribute>();
        private static readonly Func<Type, ContainerTypeAttribute> _getContainerTypeAttribute = (t) => t.GetCustomAttributes<ContainerTypeAttribute>().FirstOrDefault();
        private static ContainerTypeAttribute GetContainerTypeAttribute(Type serviceType) => _containerTypeLookup.GetOrAdd(serviceType, _getContainerTypeAttribute);

        private readonly CovariantTypeIndex _typeIndex;

        /// <summary>
        /// Implementation of the <see cref="IRootTargetContainer.TargetRegistered"/> event
        /// </summary>
        public event EventHandler<TargetRegisteredEventArgs> TargetRegistered;

        /// <summary>
        /// Implementation of the <see cref="IRootTargetContainer.TargetContainerRegistered"/> event
        /// </summary>
        public event EventHandler<TargetContainerRegisteredEventArgs> TargetContainerRegistered;

        /// <summary>
        /// The default configuration used for <see cref="TargetContainer"/> objects created via the <see cref="TargetContainer.TargetContainer(ITargetContainerConfig)"/>
        /// constructor when no configuration is explicitly passed.
        /// </summary>
        /// <remarks>The simplest way to configure all target container instances is to add/remove configs to this collection.
        ///
        /// Note also that the <see cref="OverridingTargetContainer"/> class also uses this.
        ///
        /// #### Default configurations
        ///
        /// The configurations applied by default are:
        ///
        /// - <see cref="Configuration.InjectEnumerables"/>
        /// - <see cref="Configuration.InjectArrays"/>
        /// - <see cref="Configuration.InjectLists"/>
        /// - <see cref="Configuration.InjectCollections"/>
        /// - <see cref="Configuration.InjectAutoFuncs"/>
        /// - <see cref="Configuration.InjectAutoLazies"/>
        /// - <see cref="Configuration.InjectResolveContext"/>
        /// 
        /// In most cases, these are controllable through the use of global properties such as:
        /// 
        /// - <see cref="Options.EnableEnumerableInjection"/>
        /// - <see cref="Options.EnableArrayInjection"/>
        /// - <see cref="Options.EnableListInjection"/>
        /// - <see cref="Options.EnableCollectionInjection"/>
        /// - <see cref="Options.EnableAutoFuncInjection"/> (**NOTE:** defaults to <c>false</c>)
        /// - <see cref="Options.EnableAutoLazyInjection"/> (**NOTE:** defaults to <c>false</c>)
        /// </remarks>
        public static CombinedTargetContainerConfig DefaultConfig { get; } = new CombinedTargetContainerConfig(new ITargetContainerConfig[]
        {
            Configuration.InjectEnumerables.Instance,
            Configuration.InjectArrays.Instance,
            Configuration.InjectLists.Instance,
            Configuration.InjectCollections.Instance,
            Configuration.InjectAutoFuncs.Instance,
            Configuration.InjectAutoLazies.Instance,
            Configuration.InjectResolveContext.Instance,
        });

        /// <summary>
        /// Always returns this instance.
        /// </summary>
        public override IRootTargetContainer Root => this;

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
            : this()
        {
            if (GetType() != typeof(TargetContainer))
            {
                throw new InvalidOperationException("Derived types must not use this constructor because it triggers virtual method calls via the configuration callbacks.  Please use the protected parameterless constructor instead");
            }

            (config ?? DefaultConfig).Configure(this);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TargetContainer"/> class without attaching any
        /// <see cref="ITargetContainerConfig"/> to it.  This is desirable for derived types as behaviours typically
        /// will invoke methods on this target container which are declared virtual and which are, therefore,
        /// unsafe to be called during construction.
        /// </summary>
        protected TargetContainer()
            : base()
        {
            this._typeIndex = new CovariantTypeIndex(this);
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/>,
        /// overriding the base version to extend special support for open generic types and for
        /// <see cref="ITargetContainerTypeResolver"/> options.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public override void RegisterContainer(Type type, ITargetContainer container)
        {
            // for explicit container registrations inside a TargetContainer, some container registrations
            // have to be delegated to child containers - e.g. where a container for a generic type must
            // be registered inside another which is registered against the open generic.
            // Easy way to check: see if GetTargetContainerType returns a difference type to the one passed,
            // and then autoregistering the container type if it does.  The EnsureContainer function takes
            // care of everything else.
            if (GetRegisteredContainerType(type) != type)
                EnsureContainer(type).RegisterContainer(type, container);
            else
                base.RegisterContainer(type, container);

            OnTargetContainerRegistered(container, type);
        }

        /// <summary>
        /// Raises the <see cref="TargetContainerRegistered"/> event.
        /// </summary>
        /// <param name="container">The target container that was registered</param>
        /// <param name="type">The type against which the <paramref name="container"/> was registered</param>
        protected virtual void OnTargetContainerRegistered(ITargetContainer container, Type type)
        {
            TargetContainerRegistered?.Invoke(this, new TargetContainerRegisteredEventArgs(container, type));
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
            if(target == null) throw new ArgumentNullException(nameof(target));
            if (serviceType != null && !target.SupportsType(serviceType))
            {
                throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, serviceType), nameof(target));
            }

            base.Register(target, serviceType);

            if(target is INotifyRegistrationTarget notifiableTarget)
            {
                notifiableTarget.OnRegistration(Root, serviceType ?? notifiableTarget.DeclaredType);
            }

            OnTargetRegistered(target, serviceType ?? target.DeclaredType);
        }

        /// <summary>
        /// Called when a new target has been registered in this target container.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="serviceType">Type of the service.</param>
        protected virtual void OnTargetRegistered(ITarget target, Type serviceType)
        {
            TargetRegistered?.Invoke(this, new TargetRegisteredEventArgs(target, serviceType));
        }

        /// <summary>
        /// Called to get the type that's to be used to fetch a child <see cref="ITargetContainer"/> for targets registered
        /// against a given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type - usually pulled from the <see cref="ITarget.DeclaredType"/> of a
        /// <see cref="ITarget"/> that is to be registered, or the service type passed to <see cref="ITargetContainer.Fetch(Type)"/>.</param>
        /// <returns>The redirected type, or the <paramref name="serviceType"/> if no type redirection is necessary.</returns>
        protected override Type GetRegisteredContainerType(Type serviceType)
        {
            Type toReturn;
            var attr = GetContainerTypeAttribute(serviceType);
            if (attr != null)
            {
                toReturn = attr.Type;
            }
            else
            {
                var option = this.GetOption<ITargetContainerTypeResolver>(serviceType);
                if(option != null)
                {
                    toReturn = option.GetContainerType(serviceType) ?? serviceType;
                }
                else if(ShouldUseGenericTypeDef(serviceType))
                {
                    toReturn = serviceType.GetGenericTypeDefinition();
                }
                else
                {
                    toReturn = base.GetRegisteredContainerType(serviceType);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Overrides <see cref="TargetDictionaryContainer.CreateTargetContainer(Type)"/> to provide special support
        /// for open generic types and to support <see cref="ITargetContainerFactory"/> options.
        /// </summary>
        /// <param name="targetContainerType"></param>
        /// <returns></returns>
        protected override ITargetContainer CreateTargetContainer(Type targetContainerType)
        {
            if (targetContainerType.IsGenericTypeDefinition)
                return new GenericTargetContainer(this, targetContainerType);

            return this.GetOption<ITargetContainerFactory>(targetContainerType)?.CreateContainer(targetContainerType, this) ?? base.CreateTargetContainer(targetContainerType);
        }

        ITargetContainer IRootTargetContainer.CreateTargetContainer(Type forType) => CreateTargetContainer(forType);

        Type IRootTargetContainer.GetContainerRegistrationType(Type serviceType) => GetRegisteredContainerType(serviceType);

        private static bool ShouldUseGenericTypeDef(Type serviceType)
        {
            return serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition;
        }


        void ICovariantTypeIndex.AddKnownType(Type serviceType) => this._typeIndex.AddKnownType(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCovariantTypes(Type serviceType) => this._typeIndex.GetKnownCovariantTypes(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCompatibleTypes(Type serviceType) => this._typeIndex.GetKnownCompatibleTypes(serviceType);

        TargetTypeSelector ICovariantTypeIndex.SelectTypes(Type type) => _typeIndex.SelectTypes(type);
    }
}
