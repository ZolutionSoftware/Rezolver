// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Rezolver.Compilation;
using Rezolver.Events;

namespace Rezolver
{
    using ConcurrentCache = PerResolveContextCache<ICompiledTarget>;// DefaultConcurrentPerTypeCache<ICompiledTarget>;

    /// <summary>
    /// Starting point for implementations of <see cref="IContainer"/> - only creatable through inheritance.
    /// </summary>
    /// <remarks>This class also implements <see cref="IRootTargetContainer"/> by proxying the <see cref="Targets"/> that are
    /// provided to it on construction (or created anew if not supplied).  All of those interface methods are implemented
    /// explicitly except the <see cref="Register(ITarget, Type)"/> method,  which is available through the class' public
    /// API.
    ///
    /// Note: <see cref="IContainer"/>s are generally not expected to implement <see cref="ITargetContainer"/>, and the
    /// framework will never assume they do.
    ///
    /// The reason this class does is to make it easier to create a new container and to register targets into it without
    /// having to worry about managing a separate <see cref="ITargetContainer"/> instance in your application root -
    /// because all the registration extension methods defined in classes like
    /// <see cref="RegisterTypeTargetContainerExtensions"/>, <see cref="SingletonTargetContainerExtensions"/> (plus many
    /// more) will be available to developers in code which has a reference to this class, or one derived from it.
    ///
    /// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this
    /// type will always cause a <see cref="NotSupportedException"/> to be thrown, thus preventing containers from being
    /// registered as sub target containers within an <see cref="ITargetContainer"/> via its
    /// <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/> method.
    /// </remarks>
    public class Container : IRootTargetContainer, IServiceProvider
    {
#if USEDYNAMIC
        private class DynamicContainerCache
        {
            private abstract class ContainerCache
            {
                public abstract ResolveContext GetDefaultContext<TService>();
                public abstract ResolveContext GetDefaultContext(Type serviceType);

                public abstract ICompiledTarget GetCompiled<TService>();
                public abstract ICompiledTarget GetCompiled(Type serviceType);
            }

            private abstract class TypeEntry
            {
                public abstract ResolveContext Context { get; }
                public abstract ICompiledTarget CompiledTarget { get; }
            }

            private sealed class ContainerCache<TContainer> : ContainerCache
            {
                private static Container TheContainer;
                public ContainerCache(Container container)
                {
                    TheContainer = container;
                }

                public override ICompiledTarget GetCompiled<TService>()
                {
                    return TypeEntry<TService>.Compiled.Target;
                }

                public override ICompiledTarget GetCompiled(Type serviceType)
                {
                    throw new NotImplementedException();
                }

                public override ResolveContext GetDefaultContext<TService>()
                {
                    return TypeEntry<TService>.TheContext;
                }

                public override ResolveContext GetDefaultContext(Type serviceType)
                {
                    throw new NotImplementedException();
                }

                private class TypeEntry<TService> : TypeEntry
                {
                    public static readonly ResolveContext TheContext = new ResolveContext(TheContainer, typeof(TService));

                    public static class Compiled
                    {
                        public static readonly ICompiledTarget Target = TheContainer.GetWorker(TheContext);
                    }

                    public override ResolveContext Context => TheContext;
                    public override ICompiledTarget CompiledTarget => Compiled.Target;
                }
            }

            private static int _counter = 1;
            private readonly ContainerCache _cache;
            private readonly AssemblyBuilder _dynAssembly;
            private readonly ModuleBuilder _module;

            public DynamicContainerCache(Container parent)
            {
                _dynAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"Rezolver.Dynamic.Containers.C{_counter++:00000}, Version=0.0"), AssemblyBuilderAccess.RunAndCollect);
                _module = _dynAssembly.DefineDynamicModule("module");
                var fakeContainerType = _module.DefineType($"ContainerHook", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
                _cache = (ContainerCache)Activator.CreateInstance(typeof(ContainerCache<>).MakeGenericType(fakeContainerType), parent);
            }

            public ResolveContext GetContext<TService>() => _cache.GetDefaultContext<TService>();
            public ICompiledTarget GetCompiled<TService>() => _cache.GetCompiled<TService>();
        }
#endif

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
            // note: this config object only applies itself to OverridingContainer objects, and only when the
            // EnableAutoEnumerables option is set to true in the ITargetContainer.
            //Configuration.OverridingEnumerables.Instance
        });

        private readonly ConcurrentCache _cache;
        internal ContainerScope2 _scope;
        internal PerTypeCache<ResolveContext> _cachedContexts;

#if USEDYNAMIC
        private DynamicContainerCache _contextFactory;
#endif

        /// <summary>
        /// Provides the <see cref="ITarget"/> instances that will be compiled into <see cref="ICompiledTarget"/>
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
        protected IRootTargetContainer Targets { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup
        /// when <see cref="Resolve(ResolveContext)"/> (and other operations) is called.  If not provided, a new
        /// <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available to inherited types,
        /// after construction, through the <see cref="Targets"/> property.</param>
        protected Container(IRootTargetContainer targets = null)
        {
            _cache = new ConcurrentCache(GetWorker);
            _scope = new DisposingContainerScope(this);
            Targets = targets ?? new TargetContainer();
#if USEDYNAMIC
            _contextFactory = new DynamicContainerCache(this);
#endif
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when
        /// <see cref="IContainer.Resolve(ResolveContext)"/> (and other operations) is called.  If not provided, a new
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

            _scope = new DisposingContainerScope(this);
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

        //private ResolveContext GetDefaultContext(Type serviceType)
        //{
        //    return _cachedContexts.Get(serviceType);
        //}

        //private ResolveContext GetDefaultContext<TService>()
        //{
        //    return _cachedContexts.Get(typeof(TService));
        //}

        //private ResolveContext CreateDefaultContext(Type type)
        //{
        //    return new ResolveContext(_scope, type);
        //}

        /// <summary>
        /// Gets an instance of the <see cref="ResolveContext.RequestedType"/> from the container,
        /// using the scope from the <paramref name="context"/>
        /// </summary>
        /// <param name="context">The resolve context</param>
        /// <returns>An instance of the type that was requested.</returns>
        public object Resolve(ResolveContext context)
        {
            return GetCompiledTarget(context).GetObject(context);
        }

        /// <summary>
        /// Resolves an instances of the given type using this container's scope.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType)
        {
            return Resolve(new ResolveContext(_scope, serviceType));
        }

        /// <summary>
        /// Resolves an instance of the given type for the given scope
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType, ContainerScope2 scope)
        {
            // resolve, assuming a different scope to this container's scope
            return Resolve(new ResolveContext(new ContainerScopeProxy(scope, this), serviceType));
        }

        public TService Resolve<TService>()
        {
            // our scope is bound to this container
#if !USEDYNAMIC
            return ResolveInternal<TService>(new ResolveContext(_scope, typeof(TService)));
#else
            return ResolveInternal<TService>(_contextFactory.GetContext<TService>());
#endif
        }

        public TService Resolve<TService>(ContainerScope2 scope)
        {
            // resolve, assuming a different scope to this container's scope
            return ResolveInternal<TService>(new ResolveContext(new ContainerScopeProxy(scope, this), typeof(TService)));
        }

        public IEnumerable ResolveMany(Type serviceType)
        {
            return (IEnumerable)Resolve(new ResolveContext(_scope, typeof(IEnumerable<>).MakeGenericType(serviceType)));
        }

        public IEnumerable<TService> ResolveMany<TService>()
        {
            return Resolve<IEnumerable<TService>>();
        }

        internal TService ResolveInternal<TService>(ResolveContext context)
        {
#if !USEDYNAMIC
            return (TService)GetCompiledTarget(context).GetObject(context);
#else
            return (TService)_contextFactory.GetCompiled<TService>().GetObject(context);
#endif
        }

        /// <summary>
        /// Implementation of the <see cref="IContainer.TryResolve(ResolveContext, out object)"/> method.
        ///
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
            var target = GetCompiledTarget(context);
            if (!target.IsUnresolved())
            {
                result = target.GetObject(context);
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
        /// The base definition creates a <see cref="ContainerScope2"/> with this container passed as the scope's container.
        ///
        /// Thus, the new scope is a 'root' scope.
        /// </summary>
        public ContainerScope2 CreateScope()
        {
            return _scope.CreateScope();
        }

        /// <summary>
        /// Returns <c>true</c> if a service registration can be found for the given <paramref name="serviceType"/>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public virtual bool CanResolve(Type serviceType)
        {
            return Targets.Fetch(serviceType) != null;
        }

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
        /// <see cref="IContainer"/> has started compiling targets within it can yield unpredictable results.
        ///
        /// If you create a new container and perform all your registrations before you use it, however, then everything
        /// will work as expected.
        ///
        /// Note also the other ITargetContainer interface methods are implemented explicitly so as to hide them from the
        /// list of class members.
        /// </remarks>
        public void Register(ITarget target, Type serviceType = null)
        {
            Targets.Register(target, serviceType);
        }

        /// <summary>
        /// Called by <see cref="GetCompiledTarget(ResolveContext)"/> if no valid <see cref="ITarget"/> can be
        /// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
        /// set to <c>true</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>An <see cref="ICompiledTarget"/> to be used as the result of a <see cref="Resolve(ResolveContext)"/>
        /// operation where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).
        /// </returns>
        /// <remarks>The base implementation always returns an instance of the <see cref="UnresolvedTypeCompiledTarget"/>.</remarks>
        protected virtual ICompiledTarget GetFallbackCompiledTarget(ResolveContext context)
        {
            return new UnresolvedTypeCompiledTarget(context.RequestedType);
        }

        /// <summary>
        /// Base implementation of <see cref="IContainer.GetCompiledTarget(ResolveContext)"/>.  Note that any container
        /// already defined in the <see cref="ResolveContext.Container"/> is ignored in favour of this container.
        /// </summary>
        /// <param name="context">The context containing the requested type and any scope which is currently in force.</param>
        /// <returns>Always returns a reference to a compiled target - but note that if
        /// <see cref="CanResolve(Type)"/> returns false for the same context, then the target's
        /// <see cref="ICompiledTarget.GetObject(ResolveContext)"/> method will likely throw an exception - in line with
        /// the behaviour of the <see cref="UnresolvedTypeCompiledTarget"/> class.</returns>
        public ICompiledTarget GetCompiledTarget(ResolveContext context)
        {
            // note that this container is fixed as the container in the context - regardless of the
            // one passed in.  This is important.  Scope and RequestedType are left unchanged

            return _cache.Get(context);
        }

        internal ICompiledTarget GetWorker(ResolveContext context)
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

            // if the target also supports the ICompiledTarget interface then return it, bypassing the
            // need for any direct compilation.
            // Then check whether the type of the target is compatible with the requested type - so long
            // as the requested type is not System.Object.  If so, return a ConstantCompiledTarget
            // which will simply return the target when GetObject is called.
            // note that we don't check for IDirectTarget - because that can't honour scoping rules
            if (target is ICompiledTarget compiledTarget)
            {
                return compiledTarget;
            }
            else if (context.RequestedType != typeof(object) && context.RequestedType.IsAssignableFrom(target.GetType()))
            {
                return new ConstantCompiledTarget(target, target);
            }

            var compiler = Targets.GetOption<ITargetCompiler>(target.GetType());
            if (compiler == null)
            {
                throw new InvalidOperationException($"No compiler has been configured in the Targets target container for a target of type {target.GetType()} - please use the SetOption API to set an ITargetCompiler for all target types, or for specific target types.");
            }

            return compiler.CompileTarget(target, context.ChangeContainer(newContainer: this), Targets);
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
