// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// Applying this configuration to an <see cref="ITargetContainer"/> will enable automatic injection of collection types such as
    /// <see cref="System.Collections.ObjectModel.Collection{T}"/>, <see cref="ICollection{T}"/>, <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/>
    /// and <see cref="IReadOnlyCollection{T}"/> (so long as there are no registrations for these types in the target container when the configuration is applied).
    ///
    /// Out of the box, such collections will be seeded by any objects that have been registered against the element type.
    /// </summary>
    /// <remarks>As with <see cref="InjectLists"/> this configuration object has a partner option - <see cref="Options.EnableCollectionInjection"/> - which can be used
    /// to control whether collection injection is actually enabled by this configuration.
    ///
    /// In normal operation, this configuration is applied by default to <see cref="TargetContainer"/> and <see cref="OverridingTargetContainer"/> instances via the
    /// <see cref="TargetContainer.DefaultConfig"/> combined config.  To use the <see cref="Options.EnableCollectionInjection"/> option to disable it, you can use
    /// the <see cref="Configure{TOption}"/> configuration to configure the <see cref="Options.EnableCollectionInjection"/> option to <c>false</c>.  The easiest way to do
    /// that is via the <see cref="CombinedTargetContainerConfigExtensions.ConfigureOption{TOption}(CombinedTargetContainerConfig, TOption)"/> extension method or
    /// one of its overloads.
    ///
    /// ## Requires List Injection
    ///
    /// Note that because <see cref="Collection{T}"/> and <see cref="ReadOnlyCollection{T}"/> *both* require an instance of <see cref="IList{T}"/> in their constructor,
    /// the target container MUST also be able to inject an instance of <see cref="IList{T}"/>.  The best way to do this is to ensure the <see cref="InjectLists"/> and
    /// <see cref="InjectEnumerables"/> are both enabled (which they are, by default).
    /// </remarks>
    public class InjectCollections : OptionDependentConfig<Options.EnableCollectionInjection>
    {
        private static readonly ConstructorInfo _collectionCtor = Extract.GenericConstructor((IList<object> o) => new Collection<object>(o));

        /// <summary>
        /// The one and only instance of the <see cref="InjectCollections"/> configuration.
        /// </summary>
        public static InjectCollections Instance { get; } = new InjectCollections();

        private InjectCollections() : base(false)
        {
        }

        /// <summary>
        /// Adds registrations for <see cref="Collection{T}"/> and <see cref="ReadOnlyCollection{T}"/> to the <paramref name="targets"/> (including
        /// their primary interfaces) so long as none of the types are already registered.
        /// </summary>
        /// <param name="targets"></param>
        public override void Configure(IRootTargetContainer targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (!targets.GetOption(Options.EnableCollectionInjection.Default))
            {
                return;
            }

            if (targets.Fetch(typeof(Collection<>)) != null
                || targets.Fetch(typeof(ICollection<>)) != null
                || targets.Fetch(typeof(ReadOnlyCollection<>)) != null
                || targets.Fetch(typeof(IReadOnlyCollection<>)) != null)
            {
                return;
            }

            var collectionTarget = Target.ForConstructor(_collectionCtor);
            targets.Register(collectionTarget);
            targets.Register(collectionTarget, typeof(ICollection<>));

            // there's only one constructor for ReadOnlyCollection anyway, so no need to disambiguate
            var roCollectionTarget = Target.ForType(typeof(ReadOnlyCollection<>));
            targets.Register(roCollectionTarget);
            targets.Register(roCollectionTarget, typeof(IReadOnlyCollection<>));
        }
    }
}
