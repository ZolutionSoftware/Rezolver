// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> that enables automatic resolving of the
    /// <see cref="ResolveContext"/> created for a <see cref="IContainer.Resolve(ResolveContext)"/> operation.
    /// </summary>
    /// <remarks>The implementation registers a special internal target type which implements <see cref="ICompiledTarget"/>
    /// simply by returning the context passed to its <see cref="ICompiledTarget.GetObject(ResolveContext)"/> method.
    /// 
    /// This configuration is applied to the <see cref="TargetContainer.DefaultConfig"/> automatically, and cannot be disabled
    /// through the use of options.  Either it's in the configuration, or its not.</remarks>
    public sealed class InjectResolveContext : ITargetContainerConfig
    {
        private class ResolveContextTarget : ITarget, IFactoryProvider, IFactoryProvider<ResolveContext>
        {
            private static readonly Func<ResolveContext, object> _objectFactory = c => c;
            private static readonly Func<ResolveContext, ResolveContext> _funcFactory = c => c;

            public int Id { get; } = TargetBase.NextId();

            public bool UseFallback => false;

            public Type DeclaredType => typeof(ResolveContext);

            public ScopeBehaviour ScopeBehaviour => ScopeBehaviour.None;

            public ScopePreference ScopePreference => ScopePreference.Current;

            public ITarget SourceTarget => this;

            public Func<ResolveContext, ResolveContext> Factory => _funcFactory;

            Func<ResolveContext, object> IFactoryProvider.Factory => _objectFactory;

            public bool SupportsType(Type type)
            {
                return type.IsAssignableFrom(typeof(ResolveContext));
            }
        }

        private readonly ResolveContextTarget _target = new ResolveContextTarget();

        private InjectResolveContext() { }

        /// <summary>
        /// The one and only instance of <see cref="InjectResolveContext"/>
        /// </summary>
        public static InjectResolveContext Instance { get; } = new InjectResolveContext();
        /// <summary>
        /// Attaches this behaviour to the target container, adding a registration to the <paramref name="targets"/>
        /// for the type <see cref="ResolveContext"/>.
        ///
        /// Note - if the <paramref name="targets"/> already has a registration for <see cref="ResolveContext"/>,
        /// then the behaviour DOES NOT overwrite it.
        /// </summary>
        /// <param name="targets"></param>
        public void Configure(IRootTargetContainer targets)
        {
            var existing = targets.Fetch(typeof(ResolveContext));
            if (existing == null || existing.UseFallback)
            {
                targets.Register(this._target);
            }
        }
    }
}
