using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// This configuration will enable automatic injection of <see cref="List{T}"/>, <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/>
    /// when applied to an <see cref="ITargetContainer"/>, *so long as there aren't already registrations for those types*.
    /// </summary>
    /// <remarks>
    /// If this configuration is added to a <see cref="CombinedTargetContainerConfig"/>, it can be disabled by adding 
    /// another configuration to set the <see cref="Options.EnableListInjection"/> option to <c>false</c> - typically via
    /// the <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/>
    /// method (note - the order that the configs are added doesn't matter).
    /// 
    /// The underlying behaviour relies on registrations of <see cref="IEnumerable{T}"/> to be present when
    /// the constructor for the list type is bound, as it expects to bind to the <see cref="List{T}.List(IEnumerable{T})"/> constructor.
    /// 
    /// The easiest way to achieve this is also to ensure that the
    /// <see cref="InjectEnumerables"/> configuration is enabled (which it is, by default) - which guarantees that any
    /// <see cref="IEnumerable{T}"/> can be resolved - even if empty.</remarks>
    /// <seealso cref="Options.EnableListInjection"/>
    /// <seealso cref="InjectEnumerables"/>
    public class InjectLists : OptionDependentConfig<Options.EnableListInjection>
    {
        /// <summary>
        /// The one and only instance of <see cref="InjectLists"/>
        /// </summary>
        public static InjectLists Instance { get; } = new InjectLists();
        private InjectLists() : base(false) { }

        /// <summary>
        /// Configures the passed <paramref name="targets"/> to enable auto injection of <see cref="List{T}"/>, <see cref="IList{T}"/>
        /// and <see cref="IReadOnlyList{T}"/> by registering a <see cref="Targets.GenericConstructorTarget"/> for 
        /// <see cref="List{T}"/> for all three types - so long as none of them already have a registration.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            if (!targets.GetOption(Options.EnableListInjection.Default))
                return;

            if (targets.Fetch(typeof(List<>)) != null || targets.Fetch(typeof(IList<>)) != null || targets.Fetch(typeof(IReadOnlyList<>)) != null)
                return;

            var ctor = TypeHelpers.GetConstructors(typeof(List<>))
                .SingleOrDefault(c =>
                {
                    var parms = c.GetParameters();
                    return parms.Length == 1
                        && TypeHelpers.IsGenericType(parms[0].ParameterType)
                        && parms[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                });
            var target = Target.ForType(typeof(List<>));
            targets.Register(target);
            targets.Register(target, typeof(IList<>));
            // might be an argument here for a dedication implementation to prevent casting->modification
            targets.Register(target, typeof(IReadOnlyList<>));
        }
    }
}
