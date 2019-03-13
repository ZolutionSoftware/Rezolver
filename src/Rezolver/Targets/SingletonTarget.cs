// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
    /// <summary>
    /// A target which applies the singleton pattern to any <see cref="ITarget"/>.
    ///
    /// The inner target is available from the <see cref="InnerTarget"/> property.
    /// </summary>
    public class SingletonTarget : TargetBase
    {
        internal sealed class SingletonContainer
        {
            private readonly ConcurrentDictionary<TypeAndTargetId, Lazy<object>> _cached =
                new ConcurrentDictionary<TypeAndTargetId, Lazy<object>>();

#if USEDYNAMIC
            internal abstract class SingletonCacheBase
            {
                public abstract Type GetEntryType(Type tTargetType, Type serviceType);
            }

            // as with the Container.DynamicCache, the type parameter here is a dynamic type
            // created as a hook to the singleton container instance.
            internal sealed class SingletonCache<TSingletonContainer> : SingletonCacheBase
            {
                internal static class Entry<TTypeAndTarget, TService>
                {
                    static volatile ICompiledTarget Compiled;
                    static volatile ResolveContext InitialContext;
                    static volatile bool Initialised;

                    internal static void Init(ICompiledTarget target)
                    {
                        if (Initialised) return;
                        Compiled = target;
                        Initialised = true;
                    }

                    public static TService Resolve(ResolveContext context)
                    {
                        if (Compiled == null)
                            return Instance.Value;
                        InitialContext = context;
                        return Instance.Value;
                    }

                    internal static class Instance
                    {
                        public static readonly TService Value;

                        static Instance()
                        {
                            if (InitialContext == null)
                                throw new InvalidOperationException("Initial context has not been set");

                            // guaranteed only to be executed once by the runtime.
                            Value = (TService)Compiled.GetObject(InitialContext);
                            Compiled = null;
                            InitialContext = null;
                        }
                    }
                }

                public override Type GetEntryType(Type tTargetType, Type serviceType)
                {
                    return typeof(Entry<,>).MakeGenericType(typeof(TSingletonContainer), tTargetType, serviceType);
                }
            }

            private readonly AssemblyBuilder _dynAssembly;
            private readonly ModuleBuilder _dynModule;
            private readonly SingletonCacheBase _cache;

            private ConcurrentDictionary<TypeAndTargetId, Type> _dynTargetTypeCache = new ConcurrentDictionary<TypeAndTargetId, Type>();
            private Func<TypeAndTargetId, Type> _dynTargetTypeFactory;
            internal SingletonContainer()
            {
                _dynTargetTypeFactory = InitialiseDynamicTargetType;
                (_dynAssembly, _dynModule) = DynamicAssemblyHelper.Create("Singletons");
                var fakeContainerType = _dynModule.DefineType("SingletonContainerHook", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
                _cache = (SingletonCacheBase)Activator.CreateInstance(typeof(SingletonCache<>).MakeGenericType(fakeContainerType));
            }

            private Type InitialiseDynamicTargetType(TypeAndTargetId id)
            {
                return _dynModule.DefineType($"T{id.Id}_{id.Type.TypeHandle.Value}", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
            }

            internal Type GetEntryType(ICompiledTarget compiled, int targetId, Type type)
            {
                var typeAndTargetIdType = _dynTargetTypeCache.GetOrAdd(new TypeAndTargetId(type, targetId), _dynTargetTypeFactory);
                var entryType = _cache.GetEntryType(typeAndTargetIdType, type);
                // initialise the singleton holder with the compiled target (if
                entryType.InvokeMember("Init", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, null, null, new object[] { compiled });
                return entryType;
            }

#endif

            public object GetObject(ResolveContext context, Type type, int targetId, ICompiledTarget compiled)
            {
                return _cached.GetOrAdd(new TypeAndTargetId(type, targetId), (key) =>
                {
                    return new Lazy<object>(() => compiled.GetObject(context));
                }).Value;
            }
        }

        /// <summary>
        /// Override of <see cref="TargetBase.DeclaredType"/> - always returns the DeclaredType of the <see cref="InnerTarget"/>
        /// </summary>
        /// <value>The type of the declared.</value>
        public override Type DeclaredType => InnerTarget.DeclaredType;

        /// <summary>
        /// Always returns the same behaviour as the <see cref="InnerTarget"/>
        /// </summary>
        public override ScopeBehaviour ScopeBehaviour => InnerTarget.ScopeBehaviour;

        /// <summary>
        /// Always returns <see cref="ScopePreference.Root"/>
        /// </summary>
        public override ScopePreference ScopePreference => ScopePreference.Root;
        /// <summary>
        /// Gets the inner target for this singleton.
        /// </summary>
        public ITarget InnerTarget { get; }

        /// <summary>
        /// Constructs a new instance of the <see cref="SingletonTarget"/> class.
        /// </summary>
        /// <param name="innerTarget">The target whose result (when compiled) is to be used as the singleton instance.</param>
        public SingletonTarget(ITarget innerTarget)
        {
            if(innerTarget == null) throw new ArgumentNullException(nameof(innerTarget));
            if(innerTarget is SingletonTarget) throw new ArgumentException("A SingletonTarget cannot wrap another SingletonTarget", nameof(innerTarget));

            InnerTarget = innerTarget;
        }

        /// <summary>
        /// Called to check whether a target can create an expression that builds an instance of the given <paramref name="type" />.
        ///
        /// The base implementation always passes the call on to the <see cref="InnerTarget"/>
        /// </summary>
        /// <param name="type">Required</param>
        public override bool SupportsType(Type type)
        {
            return InnerTarget.SupportsType(type);
        }
    }
}
