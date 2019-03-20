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
            private readonly PerTypeCache<Entry> _entries;

            protected DynamicCache(PerTypeCache<Entry> entries)
            {
                _entries = entries;
            }

            public abstract ResolveContext GetDefaultContext<TService>();
            public ResolveContext GetDefaultContext(Type serviceType) => _entries.Get(serviceType).ResolveContext;

            public abstract Func<ResolveContext, TService> GetFactory<TService>();
            public Func<ResolveContext, object> GetFactory(Type serviceType) => _entries.Get(serviceType).Factory;

            public abstract TService Resolve<TService>();
            public abstract TService Resolve<TService>(ResolveContext context);

            public object Resolve(Type serviceType) => _entries.Get(serviceType).Resolve();
            public object Resolve(ResolveContext context) => _entries.Get(context.RequestedType).Factory(context);

            protected class Entry
            {
                public readonly ResolveContext ResolveContext;
                public readonly Func<ResolveContext, object> Factory;

                protected Entry(ResolveContext resolveContext, Func<ResolveContext, object> factory)
                {
                    ResolveContext = resolveContext;
                    Factory = factory;
                }

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve() => Factory(ResolveContext);
            }

            private sealed class ContainerCache<TContainer> : DynamicCache
            {
                private static Container TheContainer;

                private static readonly PerTypeCache<Entry> _entries = new PerTypeCache<Entry>(t => (Entry)Activator.CreateInstance(typeof(Entry<>).MakeGenericType(typeof(TContainer), t)));

                public ContainerCache(Container container)
                    : base(_entries)
                {
                    TheContainer = container;
                }

                public override Func<ResolveContext, TService> GetFactory<TService>()
                {
                    return Entry<TService>.CompiledStrong.Factory;
                }

                public override ResolveContext GetDefaultContext<TService>()
                {
                    return Entry<TService>.Context.Value;
                }

                public override TService Resolve<TService>()
                {
                    return Entry<TService>.CompiledStrong.Factory(Entry<TService>.Context.Value);
                }

                public override TService Resolve<TService>(ResolveContext context)
                {
                    return Entry<TService>.CompiledStrong.Factory(context);
                }


                private class Entry<TService> : Entry
                {
                    public static class Context
                    {
                        public static readonly ResolveContext Value = new ResolveContext(TheContainer, typeof(TService));
                    }

                    public static class Compiled
                    {
                        public static readonly Func<ResolveContext, object> Factory = TheContainer.GetWorker(Context.Value);
                    }

                    public static class CompiledStrong
                    {
                        public static readonly Func<ResolveContext, TService> Factory = TheContainer.GetWorker<TService>(Context.Value);
                    }

                    public Entry()
                        // lift the fields out of the statics
                        : base(Context.Value, Compiled.Factory)
                    {
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