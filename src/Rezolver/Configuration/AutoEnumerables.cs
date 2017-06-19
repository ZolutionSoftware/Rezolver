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
    /// To disable it, you can either remove it from that configuration object (which then disables it for all 
    /// 
    /// If this behaviour is not attached to an <see cref="ITargetContainer"/> instance, then only explicitly
    /// registered enumerables will be able to be resolved from any <see cref="IContainer"/> built from that 
    /// target container.</remarks>
    public sealed class AutoEnumerables : ITargetContainerConfig
    {
        /// <summary>
        /// The one and only instance of the <see cref="AutoEnumerables"/> type.
        /// </summary>
        public static AutoEnumerables Instance { get; } = new AutoEnumerables();
        private AutoEnumerables()
        {
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainerConfig.Configure(ITargetContainer)"/>
        /// </summary>
        /// <param name="targets"></param>
        public void Configure(ITargetContainer targets)
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
