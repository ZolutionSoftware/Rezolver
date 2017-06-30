using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// This configuration will enable automatic injection of <see cref="List{T}"/> and <see cref="IList{T}"/>
    /// when applied to an <see cref="ITargetContainer"/>, *so long as there aren't already registrations for those types*.
    /// 
    /// If this configuration is added to a <see cref="CombinedTargetContainerConfig"/>, it can be disabled by adding 
    /// another configuration to set the <see cref="Options.EnableAutoLists"/> option to <c>false</c> - typically via
    /// the <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/>
    /// method (note - the order that the configs are added doesn't matter).
    /// </summary>
    /// <remarks>The underlying behaviour relies on registrations of <see cref="IEnumerable{T}"/> to be present when
    /// the constructor for the list type is bound, as it expects to bind to the <see cref="List{T}.List(IEnumerable{T})"/> constructor.
    /// 
    /// The easiest way to achieve this is also to ensure that the
    /// <see cref="AutoEnumerables"/> configuration is enabled (which it is, by default) - which guarantees that any
    /// <see cref="IEnumerable{T}"/> can be resolved - even if empty.</remarks>
    /// <seealso cref="Options.EnableAutoLists"/>
    /// <seealso cref="AutoEnumerables"/>
    public class AutoLists : OptionDependentConfig<Options.EnableAutoLists>
    {
        /// <summary>
        /// The one and only instance of <see cref="AutoLists"/>
        /// </summary>
        public static AutoLists Instance { get; } = new AutoLists();
        private AutoLists() : base(false) { }

        /// <summary>
        /// Configures the passed <paramref name="targets"/> to enable auto injection of <see cref="List{T}"/> and <see cref="IList{T}"/>
        /// by registering a <see cref="Targets.GenericConstructorTarget"/> for <see cref="List{T}"/> for both types.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(ITargetContainer targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption<Options.EnableAutoLists>(Options.EnableAutoLists.Default))
                return;

            if (targets.Fetch(typeof(List<>)) != null || targets.Fetch(typeof(IList<>)) != null)
                return;

            var target = Target.ForType(typeof(List<>));

            targets.Register(target);
            targets.Register(target, typeof(IList<>));

        }
    }
}
