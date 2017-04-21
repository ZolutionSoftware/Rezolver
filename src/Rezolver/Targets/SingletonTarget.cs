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
        internal class SingletonContainer
        {
            private readonly ConcurrentDictionary<Type, Lazy<object>> _cached =
                new ConcurrentDictionary<Type, Lazy<object>>();

            private readonly ConcurrentDictionary<Type, ICompiledTarget> _cachedCompiled =
                new ConcurrentDictionary<Type, ICompiledTarget>();

            private readonly ConcurrentDictionary<Type, object> _cachedObjects =
                new ConcurrentDictionary<Type, object>();

            public ICompiledTarget GetCompiled<TCompileContext>(TCompileContext context, Func<TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext: ICompileContext
            {
                return _cachedCompiled.GetOrAdd(context.TargetType, t => compiledTargetFactory(context));
            }

            public Lazy<object> GetLazy(IResolveContext context, Func<IResolveContext, object> lazyFactory)
            {
                return _cached.GetOrAdd(context.RequestedType, c => new Lazy<object>(() => lazyFactory(context)));
            }

            public object GetObject<TCompileContext>(TCompileContext context, Func<TCompileContext, ICompiledTarget> compiledTargetFactory)
                where TCompileContext : ICompileContext
            {
                return _cachedObjects.GetOrAdd(context.TargetType, c =>
                {
                    var compiled = GetCompiled(context, compiledTargetFactory);
                    return GetLazy(context.ResolveContext, rc => compiled.GetObject(rc)).Value;
                });
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
		public override ScopeBehaviour ScopeBehaviour => ScopeBehaviour.Explicit;

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
			innerTarget.MustNotBeNull("innerTarget");
			innerTarget.MustNot(t => t is SingletonTarget, "A SingletonTarget cannot wrap another SingletonTarget", nameof(innerTarget));

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
