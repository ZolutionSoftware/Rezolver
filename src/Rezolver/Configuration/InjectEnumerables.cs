// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;

namespace Rezolver.Configuration
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> which enables automatic handling of fetching
    /// targets for <see cref="IEnumerable{T}"/> based on all the targets registered for a given <c>T</c>
    /// in an <see cref="ITargetContainer"/>.
    /// </summary>
    /// <remarks>This behaviour is added to the default configuration for all <see cref="TargetContainer"/>-derived
    /// objects via the <see cref="TargetContainer.DefaultConfig"/>.
    ///
    /// To disable it, you can either remove it from that configuration object (which then disables it for all)
    /// or you can add an option configuration to it (via
    /// <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/>
    /// or similar) for the <see cref="Options.EnableEnumerableInjection"/> option, setting it to <c>false</c>.
    ///
    /// If this behaviour is not attached to an <see cref="ITargetContainer"/>, or is disabled via the
    /// <see cref="Options.EnableEnumerableInjection"/> option, then only explicitly
    /// registered enumerables will be able to be resolved by any <see cref="IContainer"/> built from that
    /// target container.
    ///
    /// #### Lazy vs Eager evaluation
    ///
    /// The enumerables created by Rezolver can be lazy or eager.  Lazy enumerables create instances as you
    /// enumerate them, and will create a unique set of instances *each time* they are enumerated (assuming
    /// no Singleton or Scoped lifetimes are in play).  Eager enumerables create all their instances up-front,
    /// and remain constant for the life of that enumerable.
    ///
    /// The <see cref="Options.LazyEnumerables"/> option (default <c>true</c>) is used to control this behaviour,
    /// and can be applied on a per-enumerable-type basis to an <see cref="ITargetContainer"/>.
    ///
    /// E.g. you can set the option to <c>false</c> for
    /// <c>IEnumerable&lt;Foo&gt;</c> - thus ensuring that all enumerables of <c>Foo</c> are eager, but leave
    /// it at its default of <c>true</c> for all other enumerable types.
    /// </remarks>
    public sealed class InjectEnumerables : OptionDependentConfig<Options.EnableEnumerableInjection>
    {
        /// <summary>
        /// The one and only instance of the <see cref="InjectEnumerables"/> type.
        /// </summary>
        public static InjectEnumerables Instance { get; } = new InjectEnumerables();

        private InjectEnumerables() : base(false)
        {
        }

        /// <summary>
        /// Implementation of <see cref="OptionDependentConfigBase.Configure(IRootTargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <remarks>
        /// This implementation registers a special target container (via <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/>)
        /// for <see cref="IEnumerable{T}"/> in passed <paramref name="targets"/>
        /// if the <see cref="Options.EnableEnumerableInjection"/> option evaluates to <c>true</c> when read from <paramref name="targets"/>.
        ///
        /// This is the default value for that option anyway, so, as the remarks section on the class states, all that's required to enable
        /// the enumerable resolving behaviour is simply to make sure this configuration object is applied to an <see cref="IRootTargetContainer"/></remarks>
        public override void Configure(IRootTargetContainer targets)
        {
            if(targets == null) throw new ArgumentNullException(nameof(targets));
            // if an option has already been set on the target container which disables automatic enumerables,
            // then do not apply the configuration.
            if (!targets.GetOption(Options.EnableEnumerableInjection.Default))
            {
                return;
            }

            if (targets.FetchContainer(typeof(IEnumerable<>)) == null)
            {
                targets.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(targets));
            }
        }
    }
}
