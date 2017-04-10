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
		//the cached compiled targets for this singleton keyed by the requested type.
		//compilers should use this so that the singleton rule can be enforced.
		//TODO: change this to be a state storage device in the container - possibly using the same
		//pattern that's been proposed for scoping (see IContainerScope, bottom of the file)
		private readonly ConcurrentDictionary<Type, ICompiledTarget> _initialisers = new ConcurrentDictionary<Type, ICompiledTarget>();

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
		/// Used to support compiled versions of this singleton
		/// </summary>
		/// <param name="declaredType">Type of object.</param>
		/// <param name="initialiserFactory">The initialiser factory.</param>
		/// <remarks>This concept is something that probably needs
		/// to move out of this type, into a more generic TargetState object or something like that.</remarks>
		public ICompiledTarget GetOrAddInitialiser(Type declaredType, Func<Type, ICompiledTarget> initialiserFactory)
		{
			declaredType.MustNotBeNull(nameof(declaredType));
			initialiserFactory.MustNotBeNull(nameof(initialiserFactory));

			return _initialisers.GetOrAdd(declaredType, initialiserFactory);
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
