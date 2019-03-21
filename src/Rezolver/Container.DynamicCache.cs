// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


#if USEDYNAMIC

using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Rezolver
{
    public partial class Container
    {
        private abstract class DynamicCache
        {
            // used for non-generic calls
            private readonly PerTypeCache<ServiceEntry> _entries;

            protected DynamicCache(PerTypeCache<ServiceEntry> entries)
            {
                _entries = entries;
            }

            protected abstract ServiceEntry<TService> GetEntry<TService>();

            public Func<ResolveContext, object> GetFactory(Type serviceType) => _entries.Get(serviceType).Factory;

            public TService Resolve<TService>() => GetEntry<TService>().Resolve();
            public TService Resolve<TService>(ResolveContext context) => GetEntry<TService>().Resolve(context);

            public object Resolve(Type serviceType) => _entries.Get(serviceType).Resolve();
            public object Resolve(ResolveContext context) => _entries.Get(context.RequestedType).Factory(context);

            protected interface IServiceEntryProvider
            {
                ServiceEntry Entry { get; }
            }

            protected interface IServiceEntryProvider<TService> : IServiceEntryProvider
            {
                new ServiceEntry<TService> Entry { get; }
            }

            protected sealed class ServiceEntry
            {
                public readonly ResolveContext DefaultContext;
                public readonly Func<ResolveContext, object> Factory;

                public ServiceEntry(ResolveContext defaultContext, Func<ResolveContext, object> factory)
                {
                    this.DefaultContext = defaultContext;
                    this.Factory = factory;
                }

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve() => Factory(DefaultContext);

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve(ResolveContext context) => Factory(context);
            }

            protected sealed class ServiceEntry<TService>
            {
                public readonly ResolveContext DefaultContext;
                public readonly Func<ResolveContext, TService> Factory;

                public ServiceEntry(ResolveContext defaultContext, Func<ResolveContext, TService> factory)
                {
                    this.DefaultContext = defaultContext;
                    this.Factory = factory;
                }

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public TService Resolve() => Factory(DefaultContext);

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public TService Resolve(ResolveContext context) => Factory(context);
            }

            private sealed class ContainerCache<TContainer> : DynamicCache
            {
                private static Container TheContainer;

                private static readonly PerTypeCache<ServiceEntry> _entries = new PerTypeCache<ServiceEntry>(
                    t => ((IServiceEntryProvider)Activator.CreateInstance(typeof(Entry<>).MakeGenericType(typeof(TContainer), t))).Entry);

                public ContainerCache(Container container)
                    : base(_entries)
                {
                    TheContainer = container;
                }

                protected override ServiceEntry<TService> GetEntry<TService>() => Entry<TService>.GenericEntry;

                private class Entry<TService> : IServiceEntryProvider<TService>
                {
                    ServiceEntry<TService> IServiceEntryProvider<TService>.Entry => GenericEntry;
                    ServiceEntry IServiceEntryProvider.Entry => NonGenericEntry;

                    public static readonly ServiceEntry NonGenericEntry;
                    public static readonly ServiceEntry<TService> GenericEntry;

                    static Entry()
                    {
                        var defaultContext = new ResolveContext(TheContainer, typeof(TService));
                        NonGenericEntry = new ServiceEntry(defaultContext, TheContainer.GetWorker(defaultContext));
                        GenericEntry = new ServiceEntry<TService>(defaultContext, TheContainer.GetWorker<TService>(defaultContext));
                    }
                }
            }

            public static DynamicCache CreateCache(Container container)
            {
                var (assembly, module) = DynamicAssemblyHelper.Create("Containers");
                var fakeContainerType = module.DefineType($"ContainerHook", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
                return (DynamicCache)Activator.CreateInstance(typeof(ContainerCache<>).MakeGenericType(fakeContainerType), container);
            }
        }
    }
}
#endif