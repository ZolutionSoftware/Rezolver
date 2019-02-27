// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

            private readonly ConcurrentDictionary<TypeAndTargetId, ICompiledTarget> _cachedCompiled =
                new ConcurrentDictionary<TypeAndTargetId, ICompiledTarget>();

            private readonly ConcurrentDictionary<TypeAndTargetId, object> _cachedObjects =
                new ConcurrentDictionary<TypeAndTargetId, object>();

            private ICompiledTarget GetCompiled<TCompileContext>(SingletonTarget target, TCompileContext context, int targetId, Func<SingletonTarget, TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext: ICompileContext
            {
                return GetCompiled(target, context, new TypeAndTargetId(context.TargetType, targetId), compiledTargetFactory);
            }

            private ICompiledTarget GetCompiled<TCompileContext>(SingletonTarget target, TCompileContext context, in TypeAndTargetId key, Func<SingletonTarget, TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext : ICompileContext
            {
                return this._cachedCompiled.GetOrAdd(key, CreateCompiledTarget);

                ICompiledTarget CreateCompiledTarget(TypeAndTargetId k) => compiledTargetFactory(target, context);
            }

            private Lazy<object> GetLazy(ResolveContext context, int targetId, Func<ResolveContext, object> lazyFactory)
            {
                return GetLazy(context, new TypeAndTargetId(context.RequestedType, targetId), lazyFactory);
            }

            private Lazy<object> GetLazy(ResolveContext context, in TypeAndTargetId key, Func<ResolveContext, object> lazyFactory)
            {
                return this._cached.GetOrAdd(key, c => new Lazy<object>(() => lazyFactory(context)));
            }

            public object GetObject<TCompileContext>(SingletonTarget target, TCompileContext context, int targetId, Func<SingletonTarget, TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext : ICompileContext
            {
                return GetObject(target, context, new TypeAndTargetId(context.TargetType, targetId), compiledTargetFactory);
            }

            public object GetObject<TCompileContext>(SingletonTarget target, TCompileContext context, in TypeAndTargetId key, Func<SingletonTarget, TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext : ICompileContext
            {
                return this._cachedObjects.GetOrAdd(key, GetObject);

                object GetObject(TypeAndTargetId k)
                {
                    var compiled = GetCompiled(target, context, k, compiledTargetFactory);
                    return GetLazy(context.ResolveContext, k, rc => compiled.GetObject(rc)).Value;
                }
            }
        }

        /// <summary>
        /// Override of <see cref="TargetBase.DeclaredType"/> - always returns the DeclaredType of the <see cref="InnerTarget"/>
        /// </summary>
        /// <value>The type of the declared.</value>
        public override Type DeclaredType => InnerTarget.DeclaredType;

        /// <summary>
        /// Always returns <see cref="ScopeBehaviour.Explicit"/>.
        /// </summary>
        public override ScopeBehaviour ScopeBehaviour => ScopeBehaviour.Implicit;

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
