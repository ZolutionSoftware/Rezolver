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
        private sealed class DynamicCache
        {
            private abstract class ContainerCache
            {
                public abstract ResolveContext GetDefaultContext<TService>();
                public abstract ResolveContext GetDefaultContext(Type serviceType);

                public abstract ICompiledTarget GetCompiled<TService>();
                public abstract ICompiledTarget GetCompiled(Type serviceType);

                public abstract TService Resolve<TService>();
                public abstract TService Resolve<TService>(ResolveContext context);

                public abstract object Resolve(Type serviceType);
                public abstract object Resolve(ResolveContext context);
            }

            private class Entry
            {
                public ResolveContext ResolveContext;
                public ICompiledTarget CompiledTarget;

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve() => CompiledTarget.GetObject(ResolveContext);
                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve(ResolveContext context) => CompiledTarget.GetObject(context);
            }

            private sealed class ContainerCache<TContainer> : ContainerCache
            {
                private static Container TheContainer;

                private static PerTypeCache<Entry>  _entries = new PerTypeCache<Entry>(t => (Entry)Activator.CreateInstance(typeof(Entry<>).MakeGenericType(typeof(TContainer), t)));

                public ContainerCache(Container container)
                {
                    TheContainer = container;
                }

                public override ICompiledTarget GetCompiled<TService>()
                {
                    return Entry<TService>.Compiled.Target;
                }

                public override ICompiledTarget GetCompiled(Type serviceType)
                {
                    return _entries.Get(serviceType).CompiledTarget;
                }

                public override ResolveContext GetDefaultContext<TService>()
                {
                    return Entry<TService>.Context.Value;
                }

                public override ResolveContext GetDefaultContext(Type serviceType)
                {
                    return _entries.Get(serviceType).ResolveContext;
                }

                public override TService Resolve<TService>()
                {
                    return (TService)Entry<TService>.Compiled.Target.GetObject(Entry<TService>.Context.Value);
                }

                public override TService Resolve<TService>(ResolveContext context)
                {
                    return (TService)Entry<TService>.Compiled.Target.GetObject(context);
                }

                public override object Resolve(Type serviceType)
                {
                    return _entries.Get(serviceType).Resolve();
                }

                public override object Resolve(ResolveContext context)
                {
                    return _entries.Get(context.RequestedType).Resolve(context);
                }

                private class Entry<TService> : Entry
                {
                    public static class Context
                    {
                        public static readonly ResolveContext Value = new ResolveContext(TheContainer, typeof(TService));
                    }

                    public static class Compiled
                    {
                        public static readonly ICompiledTarget Target = TheContainer.GetWorker(Context.Value);
                    }

                    public Entry()
                    {
                        // lift the fields out of the statics
                        ResolveContext = Context.Value;
                        CompiledTarget = Compiled.Target;
                    }
                }
            }

            private static int _counter = 1;
            private readonly ContainerCache _cache;
            private readonly AssemblyBuilder _dynAssembly;
            private readonly ModuleBuilder _module;

            public DynamicCache(Container parent)
            {
                _dynAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"Rezolver.Dynamic.Containers.C{_counter++:00000}, Version=0.0"), AssemblyBuilderAccess.RunAndCollect);
                _module = _dynAssembly.DefineDynamicModule("module");
                var fakeContainerType = _module.DefineType($"ContainerHook", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
                _cache = (ContainerCache)Activator.CreateInstance(typeof(ContainerCache<>).MakeGenericType(fakeContainerType), parent);
            }

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ResolveContext GetContext<TService>() => _cache.GetDefaultContext<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ResolveContext GetContext(Type serviceType) => _cache.GetDefaultContext(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ICompiledTarget GetCompiled<TService>() => _cache.GetCompiled<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ICompiledTarget GetCompiled(Type serviceType) => _cache.GetCompiled(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public TService Resolve<TService>() => _cache.Resolve<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public TService Resolve<TService>(ResolveContext context) => _cache.Resolve<TService>(context);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public object Resolve(Type serviceType) => _cache.Resolve(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public object Resolve(ResolveContext context) => _cache.Resolve(context);
        }
    }
}
#endif