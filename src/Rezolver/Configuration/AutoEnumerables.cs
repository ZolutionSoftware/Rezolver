using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// An <see cref="ITargetContainerConfig"/> which enables automatic handling of fetching
    /// targets for <see cref="IEnumerable{T}"/> based on all the targets registered for a given <c>T</c>
    /// in an <see cref="ITargetContainer"/>.
    /// </summary>
    /// <remarks>This behaviour is added to the default configuration for all <see cref="TargetContainer"/>-derived
    /// objects via theh <see cref="TargetContainer.DefaultConfig"/>.
    /// 
    /// To disable it, you can either remove it from that configuration object (which then disables it for all)
    /// or you can add an option configuration to it (via 
    /// <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/> 
    /// or similar) for the <see cref="Options.EnableAutoEnumerable"/> option, setting it to <c>false</c>.
    /// 
    /// If this behaviour is not attached to an <see cref="ITargetContainer"/>, or is disabled via the 
    /// <see cref="Options.EnableAutoEnumerable"/> option, then only explicitly
    /// registered enumerables will be able to be resolved by any <see cref="IContainer"/> built from that 
    /// target container.</remarks>
    public sealed class AutoEnumerables : OptionDependentConfig<Options.EnableAutoEnumerable>
    {
        /// <summary>
        /// The one and only instance of the <see cref="AutoEnumerables"/> type.
        /// </summary>
        public static AutoEnumerables Instance { get; } = new AutoEnumerables();
        private AutoEnumerables() : base(false)
        {
        }

        /// <summary>
        /// Implementation of <see cref="OptionDependentConfig{TOption}.Configure(ITargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        /// <remarks>
        /// This implementation registers a special target container (via <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/>)
        /// for <see cref="IEnumerable{T}"/> in passed <paramref name="targets"/>
        /// if the <see cref="Options.EnableAutoEnumerable"/> option evaluates to <c>true</c> when read from <paramref name="targets"/>.
        /// 
        /// This is the default value for that option anyway, so, as the remarks section on the class states, all that's required to enable
        /// the enumerable resolving behaviour is simply to make sure this configuration object is applied to an <see cref="ITargetContainer"/></remarks>
        public override void Configure(ITargetContainer targets)
        {
            targets.MustNotBeNull(nameof(targets));
            // if an option has already been set on the target container which disables automatic enumerables,
            // then do not apply the configuration.
            if (!targets.GetOption(Options.EnableAutoEnumerable.Default))
                return;
            // we can make an IDependant config which is specialised to be dependant on a particular boolean option
            // then we can make a reusable 'ConfigureOption' configuration object, which can be wrapped up behind an
            // extension method on ITargetContainerConfigCollection.
            if(targets.FetchContainer(typeof(IEnumerable<>)) == null)
                targets.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(targets));
        }
    }
}
