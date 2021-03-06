﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using Rezolver.Compilation;
using Rezolver.Events;

namespace Rezolver
{
#if !ENABLE_IL_EMIT
    using ConcurrentCache = PerResolveContextCache<Func<ResolveContext, object>>;
#endif

    /// <summary>
    /// Container through which objects can be resolved.
    /// </summary>
    /// <remarks>This class also implements <see cref="IRootTargetContainer"/> by proxying the <see cref="Targets"/> that are
    /// provided to it on construction (or created anew if not supplied).  All of those interface methods are implemented
    /// explicitly except the <see cref="Register(ITarget, Type)"/> method,  which is available through the class' public
    /// API.
    ///
    /// This makes it easier to create a new container and to register targets into it without
    /// having to worry about managing a separate <see cref="IRootTargetContainer"/> instance in your application root -
    /// because all the registration extension methods in <see cref="RootTargetContainerExtensions"/>, 
    /// and <see cref="TargetContainerExtensions"/> will be available to developers in code which has a reference to this 
    /// class, or one derived from it.
    ///
    /// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this
    /// type will always cause a <see cref="NotSupportedException"/> to be thrown, thus preventing containers from being
    /// registered as sub target containers within an <see cref="ITargetContainer"/> via its
    /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> method.
    /// </remarks>
    public partial class Container : IRootTargetContainer, IServiceProvider
    {
        /// <summary>
        /// The default container config used by all new containers.  You can add/remove configurations from this collection
        /// to change the defaults which are applied to new container instances; or you can supply an explicit configuration
        /// when creating your container.
        /// </summary>
        /// <remarks>
        /// The configurations present in this collection by default will set up the expression target compiler and extend
        /// the automatic enumerable injection functionality so that the <see cref="OverridingContainer"/> class can produce
        /// enumerables which are made up of targets registered in both the overriding container and its inner container.</remarks>
        public static CombinedContainerConfig DefaultConfig { get; } = new CombinedContainerConfig(new IContainerConfig[]
        {
            Configuration.ExpressionCompilation.Instance,
        });

        
        internal ContainerScope _scope;

#if !ENABLE_IL_EMIT
        private readonly ConcurrentCache _cache;
#else
        private readonly DynamicCache _dynCache;
#endif

        /// <summary>
        /// Provides the <see cref="ITarget"/> instances that will be compiled into factories.
        /// instances.
        /// </summary>
        /// <remarks>This class implements the <see cref="ITargetContainer"/> interface by wrapping around this instance so that
        /// an application can create an instance of <see cref="Container"/> and directly register targets into it;
        /// rather than having to create and setup the target container first.
        ///
        /// You can add registrations to this target container at any point in the lifetime of any
        /// <see cref="Container"/> instances which are attached to it.
        ///
        /// In reality, however, if any <see cref="Resolve(ResolveContext)"/> operations have been performed prior to
        /// adding more registrations, then there's no guarantee that new dependencies will be picked up - especially
        /// if the <see cref="Container"/> is being used as your application's container (which it nearly
        /// always will be).</remarks>
        protected internal IRootTargetContainer Targets { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup
        /// when <see cref="Resolve(ResolveContext)"/> (and other operations) is called.  If not provided, a new
        /// <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available to inherited types,
        /// after construction, through the <see cref="Targets"/> property.</param>
        protected Container(IRootTargetContainer targets = null)
        {
            _scope = new NonTrackingContainerScope(this);
            Targets = targets ?? new TargetContainer();
#if !ENABLE_IL_EMIT
            _cache = new ConcurrentCache(GetWorker);
#else
            _dynCache = DynamicCache.CreateCache(this);
#endif
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when
        /// <see cref="Resolve(ResolveContext)"/> (and other operations) is called.  If not provided, a new
        /// <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available
        /// to derived types, after construction, through the <see cref="Targets"/> property.</param>
        /// <param name="config">Can be null.  Configuration to apply to this container (and, potentially its <see cref="Targets"/>).
        /// If not provided, then the <see cref="DefaultConfig"/> will be used.</param>
        /// <remarks>Note to inheritors - this constructor throws an <see cref="InvalidOperationException"/> if used by a derived class,
        /// because the application of configuration to the container will likely cause virtual methods to be called.  Instead, you
        /// should declare your own constructor with the same signature which chains instead to the <see cref="Container.Container(IRootTargetContainer)"/>
        /// protected constructor; and then you should apply the configuration yourself in that constructor (falling back to
        /// <see cref="DefaultConfig"/> if null).</remarks>
        public Container(IRootTargetContainer targets = null, IContainerConfig config = null)
            : this(targets)
        {
            if (GetType() != typeof(Container))
            {
                throw new InvalidOperationException("This constructor must not be used by derived types because applying configuration will most likely trigger calls to virtual methods on this instance.  Please use the protected constructor and apply configuration explicitly in your derived class");
            }

            _scope = new NonTrackingContainerScope(this);
            (config ?? DefaultConfig).Configure(this, Targets);
        }

        event EventHandler<TargetRegisteredEventArgs> IRootTargetContainer.TargetRegistered
        {
            add
            {
                Targets.TargetRegistered += value;
            }

            remove
            {
                Targets.TargetRegistered -= value;
            }
        }

        event EventHandler<TargetContainerRegisteredEventArgs> IRootTargetContainer.TargetContainerRegistered
        {
            add
            {
                Targets.TargetContainerRegistered += value;
            }

            remove
            {
                Targets.TargetContainerRegistered -= value;
            }
        }

        /// <summary>
        /// Gets an instance of the <see cref="ResolveContext.RequestedType"/> from the container,
        /// using the scope from the <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The resolve context</param>
        /// <returns>An instance of the type that was requested.</returns>
        public object Resolve(ResolveContext context)
        {
#if !ENABLE_IL_EMIT
            return GetFactory(context)(context);
#else
            return _dynCache.Resolve(context);
#endif
        }

        /// <summary>
        /// Resolves an instance of the given <paramref name="serviceType"/> using this context's scope.
        /// </summary>
        /// <param name="serviceType">The type of service required.</param>
        /// <returns>An object compatible with the <paramref name="serviceType"/></returns>
        public object Resolve(Type serviceType)
        {
#if !ENABLE_IL_EMIT
            return Resolve(new ResolveContext(_scope, serviceType));
#else
            return _dynCache.Resolve(serviceType);
#endif
        }

        /// <summary>
        /// Resolves an instance of the given <paramref name="serviceType"/> for the given <paramref name="scope"/>.
        /// </summary>
        /// <param name="serviceType">The type of service required.</param>
        /// <param name="scope">The scope to be used for the operation.  Will be used for all scoping for the
        /// created object and any dependencies created for it.</param>
        /// <returns>An object compatible with the <paramref name="serviceType"/></returns>
        public object Resolve(Type serviceType, ContainerScope scope)
        {
            // resolve, assuming a different scope to this container's scope
            return Resolve(new ResolveContext(new ContainerScopeProxy(scope, this), serviceType));
        }

        /// <summary>
        /// Resolves an instance of <typeparamref name="TService"/> using the current container and scope.
        /// </summary>
        /// <typeparam name="TService">The type of object required.</typeparam>
        /// <returns>The instance.</returns>
        public TService Resolve<TService>()
        {
            // our scope is bound to this container
#if !ENABLE_IL_EMIT
            return ResolveInternal<TService>(new ResolveContext(_scope, typeof(TService)));
#else
            return _dynCache.Resolve<TService>();
#endif
        }

        /// <summary>
        /// Resolves an instance of <typeparamref name="TService"/> using the current container but the 
        /// supplied <paramref name="scope"/>.
        /// </summary>
        /// <typeparam name="TService">The type of object required.</typeparam>
        /// /// <param name="scope">The scope to be used for the operation.  Will be used for all scoping for the
        /// created object and any dependencies created for it.</param>
        /// <returns>The instance.</returns>
        public TService Resolve<TService>(ContainerScope scope)
        {
#if !ENABLE_IL_EMIT
            // resolve, assuming a different scope to this container's scope
            return ResolveInternal<TService>(new ResolveContext(new ContainerScopeProxy(scope, this), typeof(TService)));
#else
            return _dynCache.Resolve<TService>(new ResolveContext(new ContainerScopeProxy(scope, this), typeof(TService)));
#endif
        }

        /// <summary>
        /// Resolves an <see cref="IEnumerable{T}"/> of the <paramref name="serviceType"/> using the current container
        /// and scope.
        /// </summary>
        /// <param name="serviceType">The type of object required.</param>
        /// <returns>An enumerable containing zero or more instances of services compatible with <paramref name="serviceType"/></returns>
        public IEnumerable ResolveMany(Type serviceType)
        {
#if !ENABLE_IL_EMIT
            return (IEnumerable)Resolve(new ResolveContext(_scope, typeof(IEnumerable<>).MakeGenericType(serviceType)));
#else
            return (IEnumerable)_dynCache.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType));
#endif
        }

        /// <summary>
        /// Resolves an <see cref="IEnumerable{T}"/> of the <typeparamref name="TService"/> using the current container
        /// and scope.
        /// </summary>
        /// <typeparam name="TService">the type of object required.</typeparam>
        /// <returns>A strongly-typed enumerable containing zero or more instances of services compatible with <typeparamref name="TService"/></returns>
        public IEnumerable<TService> ResolveMany<TService>()
        {
            return Resolve<IEnumerable<TService>>();
        }

        internal TService ResolveInternal<TService>(ResolveContext context)
        {
#if !ENABLE_IL_EMIT
            return (TService)GetFactory(context)(context);
#else
            return _dynCache.Resolve<TService>(context);
#endif
        }

        /// <summary>
        /// Attempts to resolve the requested type (given on the <paramref name="context"/>, returning a boolean
        /// indicating whether the operation was successful.  If successful, then <paramref name="result"/> receives
        /// a reference to the resolved object.
        /// </summary>
        /// <param name="context">The context containing the type that's requested, any active scope and so on.</param>
        /// <param name="result">Receives a reference to the object that was resolved, if successful, or <c>null</c>
        /// if not.</param>
        /// <returns>A boolean indicating whether the operation completed successfully.</returns>
        public bool TryResolve(ResolveContext context, out object result)
        {
            var target = GetFactory(context);
            if (!target.IsUnresolved())
            {
                result = target(context);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IScopeFactory.CreateScope"/> method.
        ///
        /// The base definition creates a <see cref="ContainerScope"/> with this container passed as the scope's container.
        ///
        /// Thus, the new scope is a 'root' scope.
        /// </summary>
        public ContainerScope CreateScope()
        {
            return _scope.CreateScope();
        }

        /// <summary>
        /// Returns <c>true</c> if a service registration can be found for the given <paramref name="serviceType"/>
        /// </summary>
        /// <param name="serviceType">The type of service</param>
        /// <returns>A boolean indicating whether the service can be resolved</returns>
        public bool CanResolve(Type serviceType)
        {
            return Targets.Fetch(serviceType) != null;
        }

        /// <summary>
        /// Returns <c>true</c> if a service registration can be found for the given <typeparamref name="TService"/>
        /// </summary>
        /// <typeparam name="TService">The type of service</typeparam>
        /// <returns>A boolean indicating whether the service can be resolved</returns>
        public bool CanResolve<TService>()
        {
            return CanResolve(typeof(TService));
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/> - simply proxies the
        /// call to the target container referenced by the <see cref="Targets"/> property.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serviceType"></param>
        /// <remarks>Remember: registering new targets into an <see cref="ITargetContainer"/> after an
        /// <see cref="Container"/> has started compiling targets within it can yield unpredictable results.
        ///
        /// If you create a new container and perform all your registrations before you use it, however, then everything
        /// will work as expected.
        ///
        /// Note also the other <see cref="ITargetContainer"/> interface methods are implemented explicitly so as to hide them from the
        /// list of class members.
        /// </remarks>
        public void Register(ITarget target, Type serviceType = null)
        {
            Targets.Register(target, serviceType);
        }

        /// <summary>
        /// Called by <see cref="GetFactory(ResolveContext)"/> if no valid <see cref="ITarget"/> can be
        /// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
        /// set to <c>true</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A factory delegate to be used as the result of a <see cref="Resolve(ResolveContext)"/>
        /// operation where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).
        /// </returns>
        /// <remarks>The base implementation always returns an instance of the <see cref="UnresolvedTypeCompiledTarget"/>.</remarks>
        protected Func<ResolveContext, object> GetFallbackCompiledTarget(ResolveContext context)
        {
            return WellKnownFactories.Unresolved.Factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        protected Func<ResolveContext, TService> GetFallbackFactory<TService>(ResolveContext context)
        {
            return WellKnownFactories.Unresolved<TService>.Factory;
        }

        /// <summary>
        /// Gets the factory which will create an instance of the type indicated by <see cref="ResolveContext.RequestedType"/>
        /// of the passed <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The context containing the requested type and any scope which is currently in force.</param>
        /// <returns>Always returns a reference to a delegate - but note that if
        /// <see cref="CanResolve(Type)"/> returns false for the same context, then the target's
        /// delegate will likely throw an exception.</returns>
        public Func<ResolveContext, object> GetFactory(ResolveContext context)
        {
#if !ENABLE_IL_EMIT
            return _cache.Get(context);
#else
            return _dynCache.GetFactory(context.RequestedType);
#endif
        }

        internal Func<ResolveContext, object> GetWorker(ResolveContext context)
        {
            ITarget target = Targets.Fetch(context.RequestedType);

            if (target == null)
            {
                return GetFallbackCompiledTarget(context);
            }

            // if the entry advises us to fall back if possible, then we'll see what we get from the
            // fallback operation.  If it's NOT the unresolved target, then we'll use that instead
            if (target.UseFallback)
            {
                var fallback = GetFallbackCompiledTarget(context);
                if (!fallback.IsUnresolved())
                {
                    return fallback;
                }
            }

            // does the target actually need compilation?
            // - instance provider provides an instance via its GetInstance method
            // - factory provider provides a factory
            // - if the target is the same type or of a type compatible with the requested type, return it.
            if(target is IInstanceProvider instanceProvider)
            {
                return c => instanceProvider.GetInstance(context);
            }
            else if (target is IFactoryProvider factoryProvider)
            {
                return factoryProvider.Factory;
            }
            else if (context.RequestedType != typeof(object) && context.RequestedType.IsAssignableFrom(target.GetType()))
            {
                return c => target;
            }

            var compiler = Targets.GetOption<ITargetCompiler>(target.GetType());
            if (compiler == null)
            {
                throw new InvalidOperationException($"No compiler has been configured in the Targets target container for a target of type {target.GetType()} - please use the SetOption API to set an ITargetCompiler for all target types, or for specific target types.");
            }

            return compiler.CompileTarget(target, context.ChangeContainer(newContainer: this), Targets);
        }

        internal Func<ResolveContext, TService> GetWorker<TService>(ResolveContext context)
        {
            ITarget target = Targets.Fetch(context.RequestedType);

            if (target == null)
            {
                return GetFallbackFactory<TService>(context);
            }

            // if the entry advises us to fall back if possible, then we'll see what we get from the
            // fallback operation.  If it's NOT the unresolved target, then we'll use that instead
            if (target.UseFallback)
            {
                var fallback = GetFallbackFactory<TService>(context);
                if (!fallback.IsUnresolved())
                {
                    return fallback;
                }
            }

            // does the target actually need compilation?
            // - instance provider provides an instance via its GetInstance method
            // - factory provider provides a factory
            // - if the target is the same type or of a type compatible with the requested type, return it.
            if (target is IInstanceProvider<TService> instanceProvider)
            {
                return c => instanceProvider.GetInstance(context);
            }
            else if (target is IFactoryProvider<TService> factoryProvider)
            {
                return factoryProvider.Factory;
            }
            else if (context.RequestedType != typeof(object) 
                && context.RequestedType.IsAssignableFrom(target.GetType())
                && target is TService typedService)
            {
                return c => typedService;
            }

            var compiler = Targets.GetOption<ITargetCompiler>(target.GetType());
            if (compiler == null)
            {
                throw new InvalidOperationException($"No compiler has been configured in the Targets target container for a target of type {target.GetType()} - please use the SetOption API to set an ITargetCompiler for all target types, or for specific target types.");
            }

            return compiler.CompileTarget<TService>(target, context.ChangeContainer(newContainer: this), Targets);
        }

#region IServiceProvider implementation
        object IServiceProvider.GetService(Type serviceType)
        {
            // IServiceProvider should return null if not found - so we use TryResolve.
            TryResolve(new ResolveContext(_scope, serviceType), out var result);
            return result;
        }
#endregion


#region IRootTargetContainer explicit implementation
        ITargetContainer IRootTargetContainer.CreateTargetContainer(Type forType) => Targets.CreateTargetContainer(forType);

        Type IRootTargetContainer.GetContainerRegistrationType(Type serviceType) => Targets.GetContainerRegistrationType(serviceType);

        IRootTargetContainer ITargetContainer.Root => Targets;

        ITarget ITargetContainer.Fetch(Type type) => Targets.Fetch(type);

        IEnumerable<ITarget> ITargetContainer.FetchAll(Type type) => Targets.FetchAll(type);

        ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type) => throw new NotSupportedException();

        ITargetContainer ITargetContainer.FetchContainer(Type type) => Targets.FetchContainer(type);

        void ITargetContainer.RegisterContainer(Type type, ITargetContainer container) => Targets.RegisterContainer(type, container);

        void ICovariantTypeIndex.AddKnownType(Type serviceType) => Targets.AddKnownType(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCovariantTypes(Type serviceType) => Targets.GetKnownCovariantTypes(serviceType);

        IEnumerable<Type> ICovariantTypeIndex.GetKnownCompatibleTypes(Type serviceType) => Targets.GetKnownCompatibleTypes(serviceType);

        TargetTypeSelector ICovariantTypeIndex.SelectTypes(Type type) => Targets.SelectTypes(type);
#endregion
    }
}
